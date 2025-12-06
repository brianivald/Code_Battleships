using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabBarco; // El objeto visual del barco
    public Transform[] puntosDeSpawn; // 4 posiciones vacías donde inician los jugadores
    public GameUI gameUI;

    // Lista local de los barcos vivos
    private List<ControladorDeBarco> barcosEnJuego = new List<ControladorDeBarco>();

    void Start()
    {
        if (GameSession.jugadoresListos.Count == 0)
        {
            Debug.LogWarning("Modo prueba.");
            return;
        }

        // --- PASO 1: GENERAR LA "BARAJA" DE POSICIONES ---
        // Calculamos cuántas celdas totales hay (Ej: 10x10 = 100 celdas)
        int ancho = Tablero.Instance.ancho;
        int alto = Tablero.Instance.alto;
        int totalCeldas = ancho * alto;

        // Creamos una lista con todos los números posibles [0, 1, 2, ... 99]
        List<int> posicionesPosibles = new List<int>();
        for (int i = 0; i < totalCeldas; i++)
        {
            posicionesPosibles.Add(i);
        }

        // --- PASO 2: BARAJAR (Shuffle) ---
        // Usamos System.Random con una semilla basada en el tiempo exacto para máxima aleatoriedad
        System.Random rng = new System.Random(unchecked((int)DateTime.Now.Ticks));

        // Algoritmo Fisher-Yates para mezclar la lista
        int n = posicionesPosibles.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int valor = posicionesPosibles[k];
            posicionesPosibles[k] = posicionesPosibles[n];
            posicionesPosibles[n] = valor;
        }
        // Ahora 'posicionesPosibles' está totalmente desordenada y sin repetir números.

        // --- PASO 3: REPARTIR Y CREAR BARCOS ---
        int indexJugador = 0;
        foreach (var datosJugador in GameSession.jugadoresListos)
        {
            // Tomamos el siguiente número de la lista barajada
            int numeroAleatorio = posicionesPosibles[indexJugador];

            // Convertimos ese número único en coordenadas X, Y
            // Ejemplo: En mapa ancho 10. Si sale el 15 -> X=5, Y=1.
            int posX = numeroAleatorio % ancho;
            int posY = numeroAleatorio / ancho;

            Vector2Int posLogica = new Vector2Int(posX, posY);

            // --- DE AQUÍ PARA ABAJO ES IGUAL QUE ANTES ---

            // Calculamos posición visual
            float scale = Tablero.Instance.tamañoCelda;
            Vector3 posVisual = new Vector3(posLogica.x * scale, 0, posLogica.y * scale);

            // Instanciar
            GameObject nuevoBarcoObj = Instantiate(prefabBarco, posVisual, Quaternion.identity);

            // Configurar Controlador
            ControladorDeBarco ctrl = nuevoBarcoObj.GetComponent<ControladorDeBarco>();
            ctrl.posicionGrid = posLogica;
            ctrl.misComandos = datosJugador.rutinaDeComandos;

            Renderer rend = nuevoBarcoObj.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = datosJugador.colorBarco;

            // Registrar en Tablero
            Tablero.Instance.RegistrarObjeto(posLogica, Tablero.TipoCelda.Barco, nuevoBarcoObj);

            barcosEnJuego.Add(ctrl);
            indexJugador++;
        }

        StartCoroutine(RutinaDeBatalla());
    }

    IEnumerator RutinaDeBatalla()
    {
        yield return new WaitForSeconds(1.0f);

        bool partidaActiva = true;
        int numeroDeTurno = 1;

        while (partidaActiva)
        {
            Debug.Log($"--- INICIO TURNO {numeroDeTurno} ---");

            // 1. Contar vivos
            int vivos = 0;
            ControladorDeBarco ganador = null;

            foreach (var b in barcosEnJuego)
            {
                if (b != null && b.salud > 0)
                {
                    vivos++;
                    ganador = b;
                }
            }

            // 2. Checar Game Over
            if (vivos <= 1)
            {
                partidaActiva = false;
                if (ganador != null)
                {
                    // Obtener nombre y color de forma segura
                    string nombre = "Jugador";
                    Color color = Color.white;

                    // Intentamos obtener el color del renderer
                    Renderer rend = ganador.GetComponentInChildren<Renderer>();
                    if (rend != null) color = rend.material.color;

                    gameUI.MostrarVictoria(nombre, color);
                }
                else
                {
                    gameUI.MostrarVictoria("NADIE (Empate)", Color.white);
                }
                yield break; // Salir de la corrutina
            }

            // --- ¡ESTO ES LO QUE TE FALTABA! ---
            // 3. Ejecutar Turnos (El freno del bucle)
            foreach (var barco in barcosEnJuego)
            {
                if (barco != null && barco.salud > 0)
                {
                    barco.EjecutarSiguienteComando();
                    // ESTA LÍNEA ES VITAL: Hace que Unity espere 1 segundo antes de seguir
                    yield return new WaitForSeconds(1.0f);
                }
            }
            // ------------------------------------

            numeroDeTurno++;

            // Seguridad extra: Si no hay barcos, espera un frame para evitar congelamiento
            if (barcosEnJuego.Count == 0) yield return null;
        }
    }
}