using UnityEngine;
using UnityEngine.UI; // Necesario si usas imagenes de UI
using TMPro; // Necesario para TextMeshPro
using System.Collections; // Necesario para Corrutinas
using UnityEngine.SceneManagement;

public class ProgrammingManager : MonoBehaviour
{
    [Header("Referencias Principales")]
    public LogicBuilder logicBuilder;
    public PlayerSlot[] todosLosSlots;

    [Header("UI Alertas")]
    public TextMeshProUGUI textoAlertas; // ¡Arrastra tu nuevo texto aquí!
    public float tiempoAlerta = 3.0f; // Cuánto tiempo dura el mensaje

    private PlayerSlot slotActualmenteEditando;
    private Coroutine corrutinaAlertaActual; // Para controlar el temporizador

    // --- VALIDACIÓN Y START ---

    public void OnClickEmpezar()
    {
        // 1. Contar jugadores
        int jugadoresActivos = 0;
        int jugadoresListos = 0;

        foreach (PlayerSlot slot in todosLosSlots)
        {
            if (slot.estadoActual != PlayerSlot.SlotState.Vacio)
            {
                jugadoresActivos++;

                if (slot.estadoActual == PlayerSlot.SlotState.Listo)
                {
                    jugadoresListos++;
                }
            }
        }

        // 2. Validaciones (Tus reglas)

        // Regla A: Mínimo 2 jugadores
        if (jugadoresActivos < 2)
        {
            MostrarAlerta("Necesitas seleccionar al menos 2 jugadores para continuar.");
            return; // Detiene la función, NO carga la escena
        }

        // Regla B: Todos los activos deben estar programados (Verdes)
        if (jugadoresActivos != jugadoresListos)
        {
            MostrarAlerta("Necesitas programar rutinas para todos los jugadores activos.");
            return; // Detiene la función
        }

        // 3. Si pasa todo, guardamos y cargamos la escena
        GameSession.LimpiarDatos();

        foreach (PlayerSlot slot in todosLosSlots)
        {
            if (slot.estadoActual == PlayerSlot.SlotState.Listo)
            {
                GameSession.PlayerData data = new GameSession.PlayerData();
                data.colorBarco = slot.imgFondoCuadrado.color;
                data.rutinaDeComandos = new System.Collections.Generic.List<ShipCommand>(slot.scriptGuardado);

                // (Aquí irían las stats numéricas cuando las tengas)
                data.vidaMaxima = 100;
                data.numMinas = 5;
                data.numReparaciones = 5;

                GameSession.jugadoresListos.Add(data);
            }
        }

        SceneManager.LoadScene("BattleScene"); // Asegúrate del nombre exacto
    }

    // --- SISTEMA DE MENSAJES TEMPORALES ---

    public void MostrarAlerta(string mensaje)
    {
        // Si ya había un mensaje mostrándose, cancelamos su temporizador anterior
        if (corrutinaAlertaActual != null) StopCoroutine(corrutinaAlertaActual);

        // Iniciamos el nuevo temporizador
        corrutinaAlertaActual = StartCoroutine(RutinaMostrarAlerta(mensaje));
    }

    IEnumerator RutinaMostrarAlerta(string mensaje)
    {
        textoAlertas.text = mensaje; // Pone el texto
        textoAlertas.gameObject.SetActive(true); // Asegura que se vea

        yield return new WaitForSeconds(tiempoAlerta); // Espera 3 segundos

        textoAlertas.text = ""; // Borra el texto
        // Opcional: textoAlertas.gameObject.SetActive(false);
    }

    // ... (El resto de tus funciones IniciarProgramacion, GuardarScript, etc. se quedan igual) ...

    public void IniciarProgramacion(PlayerSlot slotQueLlama)
    {
        if (slotActualmenteEditando != null && slotActualmenteEditando != slotQueLlama)
        {
            slotActualmenteEditando.ActualizarEstado(PlayerSlot.SlotState.Activo_SinScript);
        }
        slotActualmenteEditando = slotQueLlama;
        if (logicBuilder != null) logicBuilder.ActivarPanel(slotQueLlama);
    }

    public void CancelarProgramacion(PlayerSlot slotQueLlama)
    {
        if (slotActualmenteEditando == slotQueLlama)
        {
            slotActualmenteEditando = null;
            logicBuilder.DesactivarPanel();
        }
    }

    public void GuardarScript(PlayerSlot slot, System.Collections.Generic.List<ShipCommand> script)
    {
        slot.scriptGuardado = new System.Collections.Generic.List<ShipCommand>(script);
        slot.ActualizarEstado(PlayerSlot.SlotState.Listo);
        logicBuilder.DesactivarPanel();
        slotActualmenteEditando = null;
    }

    public void RegresarAConfiguracion()
    {
        SceneManager.LoadScene("Configuracion");
    }
}