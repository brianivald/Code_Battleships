using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ScriptManager : MonoBehaviour
{
    [Header("Referencias UI Principal")]
    public TMP_Dropdown dropdownScripts;
    public Button btnGuardarScript; // El botón del disquete/guardar

    [Header("UI Guardar (Nueva)")]
    public GameObject panelNombreScript; // El panel que acabamos de crear
    public TMP_InputField inputNombre;   // Donde escriben el nombre
    public Button btnConfirmarGuardar;
    public Button btnCancelarGuardar;

    [Header("UI Cargar/Borrar (Existente)")]
    public GameObject panelOpcionesScript;
    public Button btnCargar;
    public Button btnBorrar;
    public Button btnCancelarOpciones;

    [Header("Conexión")]
    public LogicBuilder logicBuilder;
    public ProgrammingManager progManager; // ¡AHORA SÍ LO NECESITAMOS PARA LA ALERTA!

    private string filePath;
    private ScriptDataWrapper baseDeDatos;
    private int indiceSeleccionadoEnDropdown = -1;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "mis_scripts.json");

        CargarBaseDeDatos();
        ActualizarDropdown();

        // 1. Botón Principal (Abre el panel de nombre)
        btnGuardarScript.onClick.AddListener(AbrirPanelGuardar);

        // 2. Botones del Panel Guardar
        btnConfirmarGuardar.onClick.AddListener(OnClick_ConfirmarGuardado);
        btnCancelarGuardar.onClick.AddListener(() => panelNombreScript.SetActive(false));

        // 3. Dropdown y Panel Cargar/Borrar
        dropdownScripts.onValueChanged.AddListener(OnDropdownChanged);
        btnCargar.onClick.AddListener(OnClick_Cargar);
        btnBorrar.onClick.AddListener(OnClick_Borrar);
        btnCancelarOpciones.onClick.AddListener(() => panelOpcionesScript.SetActive(false));

        // Asegurar que los paneles empiecen cerrados
        panelOpcionesScript.SetActive(false);
        panelNombreScript.SetActive(false);
    }

    // --- LÓGICA DE GUARDADO CON NOMBRE ---

    void AbrirPanelGuardar()
    {
        // Limpiamos el input y abrimos el panel
        inputNombre.text = "";
        panelNombreScript.SetActive(true);
    }

    void OnClick_ConfirmarGuardado()
    {
        string nombreElegido = inputNombre.text.Trim(); // Trim quita espacios al inicio/final

        // Validación básica: No guardar sin nombre
        if (string.IsNullOrEmpty(nombreElegido))
        {
            progManager.MostrarAlerta("Error: Escribe un nombre para la rutina.");
            return;
        }

        // Obtenemos los comandos actuales
        List<ShipCommand> comandosActuales = logicBuilder.ObtenerComandosActuales();

        if (comandosActuales.Count == 0)
        {
            progManager.MostrarAlerta("Error: No puedes guardar una rutina vacía.");
            return;
        }

        // Crear y Guardar
        ScriptData nuevoScript = new ScriptData(nombreElegido, comandosActuales);
        baseDeDatos.savedScripts.Add(nuevoScript);

        GuardarEnDisco();
        ActualizarDropdown();

        // Cerrar panel y mostrar éxito
        panelNombreScript.SetActive(false);
        progManager.MostrarAlerta("¡Rutina guardada con éxito!");
    }

    // --- RESTO DEL CÓDIGO (IGUAL QUE ANTES) ---

    void GuardarEnDisco()
    {
        string json = JsonUtility.ToJson(baseDeDatos, true);
        File.WriteAllText(filePath, json);
    }

    void CargarBaseDeDatos()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            baseDeDatos = JsonUtility.FromJson<ScriptDataWrapper>(json);
        }
        else
        {
            baseDeDatos = new ScriptDataWrapper();
            CrearRutinasDefault(); // Recuerda tu función de defaults aquí
            GuardarEnDisco();
        }
    }

    // (Pega aquí tu función CrearRutinasDefault que ya tenías)
    void CrearRutinasDefault()
    {
        // --- RUTINA 1: EL TANQUE INMORTAL ---
        // Estrategia: Prioriza curarse si está herido. Si ve a alguien al frente, dispara torpedo. Si no, avanza.
        List<string> rutinaTanque = new List<string>();

        // 1. ¿Tengo poca vida? (Menos de 50) -> Me Curo
        rutinaTanque.Add("IF|Salud|<|50|0|REPARAR");

        // 2. ¿Hay enemigo al Norte? -> Torpedo
        rutinaTanque.Add("IF|Radar_N|>|0|0|TORPEDO|N");

        // 3. ¿Hay pared al Norte? -> Me muevo al Sur (Rebote)
        rutinaTanque.Add("IF|Radar_N|<|0|0|MOVER|S");

        // 4. Si nada de lo anterior pasó, avanzo implacable
        rutinaTanque.Add("MOVER|N");

        ScriptData r1 = new ScriptData();
        r1.nombre = "Tanque Inmortal";
        r1.comandosEnTexto = rutinaTanque;
        baseDeDatos.savedScripts.Add(r1);


        // --- RUTINA 2: ARTILLERÍA PESADA (Cañonero) ---
        // Estrategia: Dispara cañones en espiral cubriendo varias distancias y luego se mueve.
        List<string> rutinaArtilleria = new List<string>();

        // Dispara a distancia 3 al Norte
        rutinaArtilleria.Add("CAÑON|N|N|3");
        // Dispara a distancia 3 al Este
        rutinaArtilleria.Add("CAÑON|E|E|3");
        // Dispara a distancia 3 al Sur
        rutinaArtilleria.Add("CAÑON|S|S|3");
        // Dispara a distancia 3 al Oeste
        rutinaArtilleria.Add("CAÑON|O|O|3");
        // Se mueve para no ser un blanco fácil
        rutinaArtilleria.Add("MOVER|N");

        ScriptData r2 = new ScriptData();
        r2.nombre = "Artillería Rotatoria";
        r2.comandosEnTexto = rutinaArtilleria;
        baseDeDatos.savedScripts.Add(r2);


        // --- RUTINA 3: EL MINADOR TÁCTICO ---
        // Estrategia: Se mueve dejando un rastro de minas. Si ve una pared, gira.
        List<string> rutinaMinador = new List<string>();

        // 1. Intenta avanzar al Norte dejando una mina atrás
        rutinaMinador.Add("PLANTAR_MINA|N");

        // 2. Si hay pared al Norte, planta mina hacia el Este (Gira)
        rutinaMinador.Add("IF|Radar_N|<|0|0|PLANTAR_MINA|E");

        // 3. Si hay enemigo cerca al Sur (atrás), le dispara un Torpedo
        rutinaMinador.Add("IF|Radar_S|>|0|0|TORPEDO|S");

        // 4. Se recupera por el esfuerzo
        rutinaMinador.Add("REPARAR");

        ScriptData r3 = new ScriptData();
        r3.nombre = "Minador Táctico";
        r3.comandosEnTexto = rutinaMinador;
        baseDeDatos.savedScripts.Add(r3);


        // --- RUTINA 4: FRANCOTIRADOR (Sniper) ---
        // Estrategia: Usa el radar para confirmar blanco antes de disparar el cañón.
        List<string> rutinaSniper = new List<string>();

        // Escanea los 4 puntos cardinales. Si ve a alguien, dispara CAÑÓN preciso (distancia 2)
        rutinaSniper.Add("IF|Radar_N|>|0|0|CAÑON|N|N|2");
        rutinaSniper.Add("IF|Radar_E|>|0|0|CAÑON|E|E|2");
        rutinaSniper.Add("IF|Radar_S|>|0|0|CAÑON|S|S|2");
        rutinaSniper.Add("IF|Radar_O|>|0|0|CAÑON|O|O|2");

        // Movimiento evasivo en diagonal (Norte luego Este)
        rutinaSniper.Add("MOVER|N");
        rutinaSniper.Add("MOVER|E");

        ScriptData r4 = new ScriptData();
        r4.nombre = "Francotirador";
        r4.comandosEnTexto = rutinaSniper;
        baseDeDatos.savedScripts.Add(r4);
    }

    void ActualizarDropdown()
    {
        dropdownScripts.ClearOptions();
        List<string> opciones = new List<string>();
        opciones.Add("");

        foreach (var s in baseDeDatos.savedScripts)
        {
            opciones.Add(s.nombre);
        }
        dropdownScripts.AddOptions(opciones);
    }

    void OnDropdownChanged(int index)
    {
        if (index == 0) return;
        indiceSeleccionadoEnDropdown = index - 1;
        panelOpcionesScript.SetActive(true);
    }

    void OnClick_Cargar()
    {
        if (indiceSeleccionadoEnDropdown < 0) return;

        ScriptData data = baseDeDatos.savedScripts[indiceSeleccionadoEnDropdown];
        List<ShipCommand> comandosRecuperados = new List<ShipCommand>();

        foreach (string linea in data.comandosEnTexto)
        {
            comandosRecuperados.Add(ScriptData.DeserializarComando(linea));
        }

        logicBuilder.CargarScriptExterno(comandosRecuperados);
        panelOpcionesScript.SetActive(false);
        dropdownScripts.SetValueWithoutNotify(0);

        progManager.MostrarAlerta("Rutina cargada correctamente.");
    }

    void OnClick_Borrar()
    {
        if (indiceSeleccionadoEnDropdown < 0) return;

        baseDeDatos.savedScripts.RemoveAt(indiceSeleccionadoEnDropdown);
        GuardarEnDisco();
        ActualizarDropdown();

        panelOpcionesScript.SetActive(false);
        dropdownScripts.SetValueWithoutNotify(0);

        progManager.MostrarAlerta("Rutina eliminada.");
    }
}