using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ScriptManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Dropdown dropdownScripts;
    public Button btnGuardarScript; // El botón de arriba a la derecha
    public TMP_InputField inputNombreScript; // Necesitas un lugar para escribir el nombre al guardar

    [Header("UI Emergente")]
    public GameObject panelOpcionesScript; // Panel con botones "Cargar" y "Borrar"
    public Button btnCargar;
    public Button btnBorrar;
    public Button btnCancelar;

    [Header("Conexión")]
    public LogicBuilder logicBuilder; // Para obtener/poner los comandos
    public ProgrammingManager progManager; // Para saber qué slot estamos editando

    private string filePath;
    private ScriptDataWrapper baseDeDatos;
    private int indiceSeleccionadoEnDropdown = -1;

    void Start()
    {
        // Ruta de guardado: C:/Users/TuUsuario/AppData/LocalLow/TuEmpresa/TuJuego/mis_scripts.json
        filePath = Path.Combine(Application.persistentDataPath, "mis_scripts.json");

        CargarBaseDeDatos();
        ActualizarDropdown();

        // Conexiones
        btnGuardarScript.onClick.AddListener(OnClick_GuardarNuevo);
        dropdownScripts.onValueChanged.AddListener(OnDropdownChanged);

        // Botones del Panel Emergente
        btnCargar.onClick.AddListener(OnClick_Cargar);
        btnBorrar.onClick.AddListener(OnClick_Borrar);
        btnCancelar.onClick.AddListener(() => panelOpcionesScript.SetActive(false));

        panelOpcionesScript.SetActive(false);
    }

    // --- GUARDADO ---

    void OnClick_GuardarNuevo()
    {
        // Nota: Deberías activar un panel pequeño que pida el nombre antes de llamar a esto,
        // o usar el texto del Dropdown si permite escritura. 
        // Por simplicidad, asumiré que usas un nombre genérico o un InputField que ya tengas.

        string nombre = "Rutina " + (baseDeDatos.savedScripts.Count + 1);
        // Si tienes un input field: string nombre = inputNombreScript.text;

        // Obtenemos los comandos actuales del LogicBuilder
        // OJO: LogicBuilder necesita una forma pública de darnos la lista actual
        List<ShipCommand> comandosActuales = logicBuilder.ObtenerComandosActuales();

        if (comandosActuales.Count == 0) return; // No guardar vacíos

        ScriptData nuevoScript = new ScriptData(nombre, comandosActuales);
        baseDeDatos.savedScripts.Add(nuevoScript);

        GuardarEnDisco();
        ActualizarDropdown();
    }

    void GuardarEnDisco()
    {
        string json = JsonUtility.ToJson(baseDeDatos, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Guardado en: " + filePath);
    }

    // En ScriptManager.cs

    void CargarBaseDeDatos()
    {
        if (File.Exists(filePath))
        {
            // El archivo existe, lo leemos
            string json = File.ReadAllText(filePath);
            baseDeDatos = JsonUtility.FromJson<ScriptDataWrapper>(json);
        }
        else
        {
            // --- AQUÍ ESTÁ LA CLAVE ---
            // El archivo NO existe, así que creamos los defaults
            baseDeDatos = new ScriptDataWrapper();
            CrearRutinasDefault(); // <--- ¡Asegúrate de que esta línea esté aquí!
            GuardarEnDisco();
        }
    }

    void CrearRutinasDefault()
    {
        // --- EJEMPLO 1: Patrulla Simple ---
        List<string> comandosPatrulla = new List<string>();
        // Tienes que escribirlos en el formato interno "TIPO|PARAMETROS"
        comandosPatrulla.Add("MOVER|N");
        comandosPatrulla.Add("MOVER|N");
        comandosPatrulla.Add("MOVER|S");
        comandosPatrulla.Add("MOVER|S");

        ScriptData rutina1 = new ScriptData();
        rutina1.nombre = "Patrulla Norte-Sur";
        rutina1.comandosEnTexto = comandosPatrulla;
        baseDeDatos.savedScripts.Add(rutina1);

        // --- EJEMPLO 2: Buscador Agresivo (Con Radar) ---
        List<string> comandosAtaque = new List<string>();
        // IF(RADAR(N) > 0): TORPEDO(N) -> "IF|Radar_N|>|0|0|TORPEDO|N"
        // (Nota: el segundo 0 es el parametroExtra que ya no usamos, y el ultimo es la accion)
        comandosAtaque.Add("IF|Radar_N|>|0|0|TORPEDO|N");
        comandosAtaque.Add("MOVER|N"); // Si no hay nadie, avanza

        ScriptData rutina2 = new ScriptData();
        rutina2.nombre = "Buscador Agresivo";
        rutina2.comandosEnTexto = comandosAtaque;
        baseDeDatos.savedScripts.Add(rutina2);

        // --- EJEMPLO 3: Minador Miedoso ---
        List<string> comandosMina = new List<string>();
        comandosMina.Add("PLANTAR_MINA|S"); // Pone mina al sur (avanza al sur y la deja)
        comandosMina.Add("MOVER|E");       // Huye al este

        ScriptData rutina3 = new ScriptData();
        rutina3.nombre = "Minador Táctico";
        rutina3.comandosEnTexto = comandosMina;
        baseDeDatos.savedScripts.Add(rutina3);
    }

    // --- UI DROPDOWN ---

    void ActualizarDropdown()
    {
        dropdownScripts.ClearOptions();
        List<string> opciones = new List<string>();
        opciones.Add("Seleccionar Script..."); // Opción 0 neutra

        foreach (var s in baseDeDatos.savedScripts)
        {
            opciones.Add(s.nombre);
        }
        dropdownScripts.AddOptions(opciones);
    }

    void OnDropdownChanged(int index)
    {
        if (index == 0) return; // Seleccionó el título

        indiceSeleccionadoEnDropdown = index - 1; // Ajustamos porque el 0 es texto

        // Mostrar panel de opciones
        panelOpcionesScript.SetActive(true);
    }

    // --- ACCIONES DE CARGA/BORRADO ---

    void OnClick_Cargar()
    {
        if (indiceSeleccionadoEnDropdown < 0) return;

        ScriptData data = baseDeDatos.savedScripts[indiceSeleccionadoEnDropdown];
        List<ShipCommand> comandosRecuperados = new List<ShipCommand>();

        foreach (string linea in data.comandosEnTexto)
        {
            comandosRecuperados.Add(ScriptData.DeserializarComando(linea));
        }

        // Enviamos al LogicBuilder
        logicBuilder.CargarScriptExterno(comandosRecuperados);

        panelOpcionesScript.SetActive(false);
        // Resetear dropdown visualmente sin disparar evento
        dropdownScripts.SetValueWithoutNotify(0);
    }

    void OnClick_Borrar()
    {
        if (indiceSeleccionadoEnDropdown < 0) return;

        baseDeDatos.savedScripts.RemoveAt(indiceSeleccionadoEnDropdown);
        GuardarEnDisco();
        ActualizarDropdown();

        panelOpcionesScript.SetActive(false);
        dropdownScripts.SetValueWithoutNotify(0);
    }
}