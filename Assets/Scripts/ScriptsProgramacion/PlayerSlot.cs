using UnityEngine;
using UnityEngine.UI; // Necesario para Button, Image, etc.
using TMPro; // Si vas a cambiar el texto "Programar"
using System.Collections.Generic; // Necesario para List<>

// Asegúrate de que este script esté en el objeto raíz de CADA jugador
public class PlayerSlot : MonoBehaviour
{
    // Define los estados posibles de este slot
    public enum SlotState { Vacio, Activo_SinScript, Editando, Listo }
    public SlotState estadoActual;

    [Header("Componentes Visuales (Arrastra aquí)")]
    public Image imgFondoCuadrado;
    public Image imgIconoBarco;
    public Button btnAnadir;         // El botón '+'
    public Button btnEliminar;       // El botón de basura
    public Button btnProgramar;

    [Header("Colores Botón Programar")]
    public Image imgBotonProgramar;   // La imagen del botón 'Programar' para cambiar su color
    public Color colorGris = new Color(0.8f, 0.8f, 0.8f); // Gris
    public Color colorRojo = Color.red;
    public Color colorAmarillo = Color.yellow;
    public Color colorVerde = Color.green; // Para el futuro
    public TMP_Text txtBotonProgramar;

    public List<ShipCommand> scriptGuardado = new List<ShipCommand>();

    [Header("Conexión al Manager")]
    // Arrastra tu objeto "GameManager" aquí
    public ProgrammingManager manager;

    // --- Métodos de Unity ---

    void Start()
    {
        // Conectamos los botones a las funciones de este script
        btnAnadir.onClick.AddListener(OnAnadir);
        btnEliminar.onClick.AddListener(OnEliminar);
        btnProgramar.onClick.AddListener(OnProgramar);

        // Estado inicial al empezar el juego
        ActualizarEstado(SlotState.Vacio);
    }

    // --- Control de Estado ---

    public void ActualizarEstado(SlotState nuevoEstado)
    {
        estadoActual = nuevoEstado;

        switch (estadoActual)
        {
            case SlotState.Vacio:
                // "en un inicio este opaco... menos el de +"
                SetGrupoOpacidad(0.5f); // Pone opaco el fondo y el barco
                btnProgramar.interactable = false;
                imgBotonProgramar.color = colorGris;
                txtBotonProgramar.text = "Programar";

                btnAnadir.gameObject.SetActive(true);   // Muestra el '+'
                btnEliminar.gameObject.SetActive(false); // Oculta la basura
                break;

            case SlotState.Activo_SinScript:
                // "se le quita lo opaco... el boton de programar pasa de gris a rojo"
                SetGrupoOpacidad(1.0f); // Opacidad normal
                btnProgramar.interactable = true;
                imgBotonProgramar.color = colorRojo;
                txtBotonProgramar.text = "Programar";

                btnAnadir.gameObject.SetActive(false);  // Oculta el '+'
                btnEliminar.gameObject.SetActive(true);  // Muestra la basura
                break;

            case SlotState.Editando:
                // "pasa a amarillo y se habilita el area de prograamcion"
                SetGrupoOpacidad(1.0f);
                btnProgramar.interactable = true; // Sigue siendo interactuable
                imgBotonProgramar.color = colorAmarillo;
                txtBotonProgramar.text = "Programando";

                btnAnadir.gameObject.SetActive(false);
                btnEliminar.gameObject.SetActive(true);
                break;

            case SlotState.Listo:
                // (Para el futuro)
                SetGrupoOpacidad(1.0f);
                btnProgramar.interactable = true;
                imgBotonProgramar.color = colorVerde;
                txtBotonProgramar.text = "Programado";

                btnAnadir.gameObject.SetActive(false);
                btnEliminar.gameObject.SetActive(true);
                break;
        }
    }

    // --- Funciones de los Botones ---

    void OnAnadir()
    {
        // Al presionar '+', pasamos al estado "Activo"
        ActualizarEstado(SlotState.Activo_SinScript);
    }

    void OnEliminar()
    {
        // Al presionar 'basura', volvemos al estado "Vacío"
        ActualizarEstado(SlotState.Vacio);
        scriptGuardado.Clear(); // Limpia la lista de comandos

        // Avisa al manager que este jugador ya no está editando
        if (manager != null)
        {
            manager.CancelarProgramacion(this);
        }
    }

    void OnProgramar()
    {
        // Al presionar 'Programar', pasamos a "Editando"
        ActualizarEstado(SlotState.Editando);

        // AVISAMOS AL MANAGER: "¡Oye, quiero programar!"
        if (manager != null)
        {
            manager.IniciarProgramacion(this);
        }
    }

    // --- Función Auxiliar ---

    // Cambia la opacidad de los elementos principales
    void SetGrupoOpacidad(float alpha)
    {
        // Fondo
        Color tempColor = imgFondoCuadrado.color;
        tempColor.a = alpha;
        imgFondoCuadrado.color = tempColor;

        // Barco
        tempColor = imgIconoBarco.color;
        tempColor.a = alpha;
        imgIconoBarco.color = tempColor;

        // Botón Programar (solo su opacidad, no su color RGB)
        tempColor = imgBotonProgramar.color;
        tempColor.a = alpha;
        imgBotonProgramar.color = tempColor;

        // Botón Eliminar
        tempColor = btnEliminar.GetComponent<Image>().color;
        tempColor.a = alpha;
        btnEliminar.GetComponent<Image>().color = tempColor;
    }
}