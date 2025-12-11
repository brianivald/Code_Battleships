using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Tablero;

public class ControladorDeBarco : MonoBehaviour
{
    [Header("Identificación")]
    public int idJugador; // <--- NUEVO: Para saber a quién sumar el punto
    [Header("Configuración")]
    public float velocidadAnimacion = 2.0f;
    public int salud = 0;
    public int minasRestantes = 0;
    public int reparacionesRestantes = 0;

    [Header("Referencias Visuales")]
    public GameObject prefabBala;
    public GameObject prefabTorpedo;
    public GameObject prefabMina;

    // Posicion logica en el tablero (X, Y)
    public Vector2Int posicionGrid;
    public Direccion orientacionActual = Direccion.Norte;

    // Referencia a la lista de comandos que este barco debe ejecutar
    public List<ShipCommand> misComandos;
    private int indiceComando = 0;

    void Start()
    {
        salud = GameSession.saludMaxima;
        minasRestantes = GameSession.minasMaximas;
        reparacionesRestantes = GameSession.reparacionesMaximas;
    }

    // --- Logica de Ejecucion de Turno ---

    public void EjecutarSiguienteComando()
    {
        if (misComandos == null || misComandos.Count == 0) return;

        ShipCommand comando = misComandos[indiceComando];
        comando.Ejecutar(this);

        indiceComando++;
        if (indiceComando >= misComandos.Count)
        {
            indiceComando = 0;
        }
    }

    // --- ACCIONES DE MOVIMIENTO ---

    public void MoverHacia(Direccion dir)
    {
        Vector2Int vectorDir = ObtenerVectorDireccion(dir);
        Vector2Int nuevaPos = posicionGrid + vectorDir;

        // Verificamos con el Tablero si se puede pisar (Vacio o Mina)
        if (Tablero.Instance.EsCaminable(nuevaPos))
        {
            // 1. Checar si pisamos una mina ANTES de movernos
            if (Tablero.Instance.GetContenido(nuevaPos) == Tablero.TipoCelda.Mina)
            {
                Debug.Log("BOOM! Has pisado una mina.");
                RecibirDano(50);

                // Destruir la mina visual y logica
                GameObject minaObj = Tablero.Instance.GetObjetoEn(nuevaPos);
                if (minaObj != null) Destroy(minaObj);
                Tablero.Instance.EliminarObjeto(nuevaPos);
            }

            // 2. Mover L�gicamente
            Tablero.Instance.MoverObjeto(posicionGrid, nuevaPos, Tablero.TipoCelda.Barco, this.gameObject);
            posicionGrid = nuevaPos;

            // 3. Mover Visualmente
            float scale = Tablero.Instance.tamañoCelda;
            transform.position = new Vector3(posicionGrid.x * scale, transform.position.y, posicionGrid.y * scale);

            RotarVisualmente(dir);
            Debug.Log($"Barco movido a {posicionGrid}");
        }
        else
        {
            // --- AQU� EST� EL CAMBIO PARA EL DA�O ---

            TipoCelda obstaculo = Tablero.Instance.GetContenido(nuevaPos);

            // CASO 1: Choque contra PARED (Fuera del mapa)
            if (obstaculo == Tablero.TipoCelda.FueraDeLimites)
            {
                Debug.Log($"¡CRASH! {gameObject.name} chocó contra la costa.");
                RecibirDano(15); // Da�o por chocar con tierra
            }

            // CASO 2: Choque contra OTRO BARCO (Ramming)
            else if (obstaculo == Tablero.TipoCelda.Barco)
            {
                Debug.Log($"¡PUM! {gameObject.name} embistió a otro barco.");

                // 1. Me hago da�o a m� mismo
                RecibirDano(20);

                // 2. Le hago da�o al que golpe�
                GameObject otroBarcoObj = Tablero.Instance.GetObjetoEn(nuevaPos);
                if (otroBarcoObj != null)
                {
                    ControladorDeBarco enemigo = otroBarcoObj.GetComponent<ControladorDeBarco>();
                    if (enemigo != null)
                    {
                        enemigo.RecibirDano(20); // El otro tambi�n sufre
                    }
                }
            }

            // Efecto visual de rebote o choque (opcional)
            RotarVisualmente(dir);
        }
    }

    // --- ACCIONES DE COMBATE ---

    public void DispararCanon(Direccion dir1, Direccion dir2, int distancia)
    {
        // Calcular destino
        Vector2Int vector1 = ObtenerVectorDireccion(dir1);
        Vector2Int vector2 = ObtenerVectorDireccion(dir2);
        Vector2Int offset = (vector1 + vector2) * distancia;
        Vector2Int coordenadaObjetivo = posicionGrid + offset;

        Debug.Log($"DISPARANDO CA��N a {coordenadaObjetivo}");

        // Efecto Visual (Bala)
        if (prefabBala != null)
        {
            float scale = Tablero.Instance.tamañoCelda;
            Vector3 destinoVisual = new Vector3(coordenadaObjetivo.x * scale, transform.position.y, coordenadaObjetivo.y * scale);

            GameObject bala = Instantiate(prefabBala, transform.position, Quaternion.identity);
            bala.GetComponent<Proyectil>().Configurar(destinoVisual);
        }

        // L�gica de Impacto
        if (Tablero.Instance.EsCoordenadaValida(coordenadaObjetivo))
        {
            if (Tablero.Instance.GetContenido(coordenadaObjetivo) == Tablero.TipoCelda.Barco)
            {
                GameObject objetoGolpeado = Tablero.Instance.GetObjetoEn(coordenadaObjetivo);
                if (objetoGolpeado != null)
                {
                    ControladorDeBarco enemigo = objetoGolpeado.GetComponent<ControladorDeBarco>();
                    if (enemigo != null) enemigo.RecibirDano(GameSession.danoCanon);
                }
            }
        }
    }

    public void LanzarTorpedo(Direccion dir)
    {
        RotarVisualmente(dir);
        Debug.Log("Lanzando Torpedo al " + dir);

        Vector2Int vectorDir = ObtenerVectorDireccion(dir);
        Vector2Int posImpacto = posicionGrid;
        bool impactoEncontrado = false;
        GameObject objetivoGolpeado = null;

        // Raycast l�gico para encontrar impacto
        for (int i = 1; i < 20; i++)
        {
            Vector2Int posTest = posicionGrid + (vectorDir * i);

            if (!Tablero.Instance.EsCoordenadaValida(posTest)) // Pared
            {
                posImpacto = posTest;
                impactoEncontrado = true;
                break;
            }
            if (Tablero.Instance.GetContenido(posTest) == Tablero.TipoCelda.Barco) // Barco
            {
                posImpacto = posTest;
                objetivoGolpeado = Tablero.Instance.GetObjetoEn(posTest);
                impactoEncontrado = true;
                break;
            }
        }

        // Efecto Visual
        if (prefabTorpedo != null)
        {
            float scale = Tablero.Instance.tamañoCelda;
            GameObject torpedo = Instantiate(prefabTorpedo, transform.position, Quaternion.identity);
            Vector3 destinoVisual = new Vector3(posImpacto.x * scale, transform.position.y, posImpacto.y * scale);

            torpedo.GetComponent<Proyectil>().Configurar(destinoVisual);
            torpedo.transform.LookAt(destinoVisual);
        }

        // Da�o
        if (impactoEncontrado && objetivoGolpeado != null)
        {
            ControladorDeBarco enemigo = objetivoGolpeado.GetComponent<ControladorDeBarco>();
            if (enemigo != null) enemigo.RecibirDano(GameSession.danoTorpedo);
        }
    }

    public void PlantarMina(Direccion dir)
    {
        if (minasRestantes <= 0)
        {
            MoverHacia(dir);
            return;
        }

        Vector2Int casillaAnterior = posicionGrid;

        // Intentamos movernos (aqu� ocurre el choque si hay obst�culo)
        MoverHacia(dir);

        // Verificamos si logramos movernos
        if (posicionGrid != casillaAnterior)
        {
            // L�gica NORMAL (Sali� bien)
            Tablero.Instance.RegistrarObjeto(casillaAnterior, Tablero.TipoCelda.Mina, null);
            if (prefabMina != null)
            {
                float scale = Tablero.Instance.tamañoCelda;
                Vector3 posVisual = new Vector3(casillaAnterior.x * scale, 0.2f, casillaAnterior.y * scale);
                GameObject minaObj = Instantiate(prefabMina, posVisual, Quaternion.identity);
                Tablero.Instance.RegistrarObjeto(casillaAnterior, Tablero.TipoCelda.Mina, minaObj);
            }
            minasRestantes--;
            Debug.Log($"Mina plantada. Restantes: {minasRestantes}");
        }
        else
        {
            // --- L�GICA KAMIKAZE (Sali� mal) ---
            Debug.Log("�CR�TICO! Chocaste y se te cay� la mina en los pies.");

            // 1. Gastas la mina
            minasRestantes--;

            // 2. Te comes el da�o instant�neo
            RecibirDano(50); // �PUM!

            // 3. (Opcional) Instanciar explosi�n visual aqu�
            // Instantiate(prefabExplosion, transform.position, ...);
        }
    }

   public void Reparar()
    {
        // 1. Checar si tengo "kits de reparación" disponibles
        if (reparacionesRestantes > 0)
        {
            // 2. Calcular curación (Digamos que curas un 20% de la salud máxima o fijo 20)
            int cantidadCuracion = 20; 
            int vidaAnterior = salud;
            
            salud += cantidadCuracion;

            // 3. Tope de salud (No pasar de la Salud Máxima Configurada)
            if (salud > GameSession.saludMaxima) 
            {
                salud = GameSession.saludMaxima;
            }

            // 4. Gastar el uso
            reparacionesRestantes--;

            Debug.Log($"Reparando... Vida: {vidaAnterior} -> {salud}. Kits restantes: {reparacionesRestantes}");
            
            // Efecto visual
            StartCoroutine(EfectoColor(Color.green));
        }
        else
        {
            // 5. Si no quedan usos
            Debug.Log("¡No te quedan reparaciones! El comando se desperdició.");
            
            // Opcional: Podrías hacer un sonido de "Error" visual aquí
        }
    }

    // --- SALUD Y DA�O ---

    public void RecibirDano(int cantidad)
    {
        salud -= cantidad;
        Debug.Log($"{gameObject.name} recibi� da�o. Salud: {salud}");
        StartCoroutine(EfectoColor(Color.red));

        if (salud <= 0) Morir();
    }

    void Morir()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        Tablero.Instance.EliminarObjeto(posicionGrid);
        Destroy(gameObject);
    }

    // --- AI Y RADAR ---

   // En ControladorDeBarco.cs

    // Modificamos GetCondicionValor para asegurar que el rango máximo sea 10 (regla del juego)
    public int GetCondicionValor(CondicionTipo tipo, int parametroExtra)
    {
        if (tipo == CondicionTipo.Salud) return salud;

        Direccion dir = Direccion.Norte;
        switch (tipo)
        {
            case CondicionTipo.Radar_N: dir = Direccion.Norte; break;
            case CondicionTipo.Radar_S: dir = Direccion.Sur; break;
            case CondicionTipo.Radar_E: dir = Direccion.Este; break;
            case CondicionTipo.Radar_O: dir = Direccion.Oeste; break;
        }
        
        // Regla: Escanear hasta 10 casillas máximo
        return EscanearGrid(dir, 10); 
    }

    private int EscanearGrid(Direccion dir, int distanciaMax)
    {
        Vector2Int vectorDir = ObtenerVectorDireccion(dir);

        // Iteramos desde la casilla 1 hasta la 10 (o distanciaMax)
        for (int i = 1; i <= distanciaMax; i++)
        {
            Vector2Int posRevisar = posicionGrid + (vectorDir * i);
            Tablero.TipoCelda contenido = Tablero.Instance.GetContenido(posRevisar);

            // CASO A: OBSTÁCULO (Pared o Mina) -> Devuelve distancia NEGATIVA (-i)
            // El usuario pidió no distinguir entre mina y pared, ambos son obstáculos.
            if (contenido == Tablero.TipoCelda.FueraDeLimites || 
                contenido == Tablero.TipoCelda.Pared || 
                contenido == Tablero.TipoCelda.Mina) 
            {
                return -i; // Ej: Si está a 3 casillas, devuelve -3
            }

            // CASO B: BARCO -> Devuelve distancia POSITIVA (i)
            if (contenido == Tablero.TipoCelda.Barco)
            {
                return i; // Ej: Si está a 5 casillas, devuelve 5
            }
        }

        // Si no encontró nada en el rango de 10 casillas
        return 0;
    }

    // --- UTILIDADES ---

    private void RotarVisualmente(Direccion dir)
    {
        float angulo = 0;
        switch (dir)
        {
            case Direccion.Norte: angulo = 0; break;
            case Direccion.Este: angulo = 90; break;
            case Direccion.Sur: angulo = 180; break;
            case Direccion.Oeste: angulo = 270; break;
        }
        transform.rotation = Quaternion.Euler(0, angulo, 0);
        orientacionActual = dir;
    }

    private Vector2Int ObtenerVectorDireccion(Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Norte: return new Vector2Int(0, 1);
            case Direccion.Sur: return new Vector2Int(0, -1);
            case Direccion.Este: return new Vector2Int(1, 0);
            case Direccion.Oeste: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }

    IEnumerator EfectoColor(Color color)
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = color;
            yield return new WaitForSeconds(0.5f);
            rend.material.color = original;
        }
    }
}