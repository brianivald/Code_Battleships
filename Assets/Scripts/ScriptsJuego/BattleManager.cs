using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement; // Necesario para recargar la escena

public class BattleManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabBarco;
    public GameUI gameUI;
    
    [Header("Torneo")]
    public int totalRondas = 3;

    // --- VARIABLES GLOBALES (Sobreviven al reinicio de escena) ---
    public static int rondaActual = 1;
    public static int[] puntosPorJugador; // Guardará las victorias [0, 1, 0, 2...]
    public static bool torneoIniciado = false;

    private List<ControladorDeBarco> barcosEnJuego = new List<ControladorDeBarco>();

    void Start()
    {
        if (GameSession.jugadoresListos.Count == 0)
        {
            Debug.LogWarning("Modo prueba.");
            return;
        }

        // 1. INICIALIZAR TORNEO (Solo si es la primera vez o venimos del menú)
        if (!torneoIniciado)
        {
            rondaActual = 1;
            puntosPorJugador = new int[GameSession.jugadoresListos.Count]; // Array de ceros
            torneoIniciado = true;
            Debug.Log("¡INICIO DEL TORNEO!");
        }

        Debug.Log($"--- COMENZANDO RONDA {rondaActual} DE {totalRondas} ---");

        // 2. GENERAR POSICIONES ALEATORIAS (Sistema de Baraja)
        int ancho = Tablero.Instance.ancho;
        int alto = Tablero.Instance.alto;
        List<int> posicionesPosibles = new List<int>();
        for (int i = 0; i < ancho * alto; i++) posicionesPosibles.Add(i);

        System.Random rng = new System.Random(unchecked((int)DateTime.Now.Ticks));
        int n = posicionesPosibles.Count;
        while (n > 1) { n--; int k = rng.Next(n + 1); int val = posicionesPosibles[k]; posicionesPosibles[k] = posicionesPosibles[n]; posicionesPosibles[n] = val; }

        // 3. CREAR BARCOS
        int indexJugador = 0;
        foreach (var datosJugador in GameSession.jugadoresListos)
        {
            int numeroAleatorio = posicionesPosibles[indexJugador];
            int posX = numeroAleatorio % ancho;
            int posY = numeroAleatorio / ancho;
            Vector2Int posLogica = new Vector2Int(posX, posY);

            float scale = Tablero.Instance.tamañoCelda;
            Vector3 posVisual = new Vector3(posLogica.x * scale, 0, posLogica.y * scale);

            GameObject nuevoBarcoObj = Instantiate(prefabBarco, posVisual, Quaternion.identity);
            
            ControladorDeBarco ctrl = nuevoBarcoObj.GetComponent<ControladorDeBarco>();
            ctrl.posicionGrid = posLogica;
            ctrl.misComandos = datosJugador.rutinaDeComandos; // Mismos códigos
            ctrl.idJugador = indexJugador; // <--- ASIGNAMOS EL ID
            
            Renderer rend = nuevoBarcoObj.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = datosJugador.colorBarco;

            Tablero.Instance.RegistrarObjeto(posLogica, Tablero.TipoCelda.Barco, nuevoBarcoObj);
            barcosEnJuego.Add(ctrl);
            indexJugador++;
        }

        StartCoroutine(RutinaDeBatalla());
    }

    IEnumerator RutinaDeBatalla()
    {
        yield return new WaitForSeconds(1.0f);
        bool rondaActiva = true;

        // Opcional: Mostrar letrero de "Ronda X" aquí usando gameUI

        while (rondaActiva)
        {
            // 1. Contar vivos
            int vivos = 0;
            ControladorDeBarco sobreviviente = null;

            foreach (var b in barcosEnJuego)
            {
                if (b != null && b.salud > 0)
                {
                    vivos++;
                    sobreviviente = b;
                }
            }

            // 2. VERIFICAR FIN DE RONDA
            if (vivos <= 1)
            {
                rondaActiva = false;
                
                // A. Asignar Punto
                string mensajeRonda = "";
                Color colorMensaje = Color.white;

                if (sobreviviente != null)
                {
                    // Sumamos punto al ganador
                    puntosPorJugador[sobreviviente.idJugador]++;
                    
                    mensajeRonda = $"¡Ronda {rondaActual} ganada por Jugador {sobreviviente.idJugador + 1}!";
                    if(sobreviviente.GetComponentInChildren<Renderer>())
                        colorMensaje = sobreviviente.GetComponentInChildren<Renderer>().material.color;
                }
                else
                {
                    mensajeRonda = $"¡Ronda {rondaActual}: Empate total!";
                }

                // B. Decidir: ¿Siguiente ronda o Final?
                if (rondaActual < totalRondas)
                {
                    // --- CASO: FALTAN RONDAS ---
                    Debug.Log(mensajeRonda + " Reiniciando en 3 segundos...");
                    
                    // Usamos la UI para mostrar quién ganó la ronda brevemente
                    gameUI.MostrarVictoria(mensajeRonda + "\nSiguiente Ronda...", colorMensaje);
                    
                    yield return new WaitForSeconds(3.0f); // Pausa para leer
                    
                    gameUI.OcultarPanel(); // Importante crear este método en GameUI
                    rondaActual++;
                    
                    // REINICIAR ESCENA (Nuevas posiciones, mismos códigos)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                else
                {
                    // --- CASO: TORNEO TERMINADO ---
                    Debug.Log("--- FIN DEL TORNEO ---");
                    DeterminarGanadorFinal();
                }
                
                yield break;
            }

            // 3. Ejecutar Turnos
            foreach (var barco in barcosEnJuego)
            {
                if (barco != null && barco.salud > 0)
                {
                    barco.EjecutarSiguienteComando();
                    yield return new WaitForSeconds(1.0f);
                }
            }
            if (barcosEnJuego.Count == 0) yield return null;
        }
    }

    void DeterminarGanadorFinal()
    {
        // Buscar quién tiene más puntos
        int maxPuntos = -1;
        int indiceGanador = -1;
        bool empate = false;

        for (int i = 0; i < puntosPorJugador.Length; i++)
        {
            if (puntosPorJugador[i] > maxPuntos)
            {
                maxPuntos = puntosPorJugador[i];
                indiceGanador = i;
                empate = false;
            }
            else if (puntosPorJugador[i] == maxPuntos)
            {
                empate = true;
            }
        }

        // Mostrar UI Final
        if (empate || indiceGanador == -1)
        {
            gameUI.MostrarVictoria($"TORNEO TERMINADO\n¡EMPATE CON {maxPuntos} PUNTOS!", Color.white);
        }
        else
        {
            // Intentar recuperar el color del ganador original
            Color colorGanador = GameSession.jugadoresListos[indiceGanador].colorBarco;
            gameUI.MostrarVictoria($"¡CAMPEÓN DEL TORNEO!\nJUGADOR {indiceGanador + 1}\n({maxPuntos} victorias)", colorGanador);
        }

        // Reseteamos el torneo para la próxima vez que jueguen desde el menú
        torneoIniciado = false;
        rondaActual = 1;
    }
}