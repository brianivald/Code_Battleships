using UnityEngine;
using TMPro; // Necesario para los Inputs y Dropdown
using UnityEngine.SceneManagement;

public class ConfigMenu : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TMP_Dropdown dropdownTamano;
    public TMP_InputField inputMinas;
    public TMP_InputField inputSalud;
    public TMP_InputField inputReparaciones;
    public TMP_InputField inputDanoCanon;
    public TMP_InputField inputDanoTorpedo;

    // Esta función la llamarás desde el botón "Continuar" o "Guardar"
    public void GuardarYContinuar()
    {
        // 1. Guardar Tamaño del Tablero
        // Asumiendo que tus opciones son: 0->5x5, 1->8x8, 2->10x10
        int index = dropdownTamano.value;
        if (index == 0) GameSession.tamanoTablero = 5;
        else if (index == 1) GameSession.tamanoTablero = 8;
        else if (index == 2) GameSession.tamanoTablero = 10;

        // 2. Guardar valores numéricos (Parsear texto a int)
        // Usamos int.Parse, asegúrate de que los campos NO estén vacíos en Unity
        if (!string.IsNullOrEmpty(inputMinas.text)) 
            GameSession.minasMaximas = int.Parse(inputMinas.text);
            
        if (!string.IsNullOrEmpty(inputSalud.text)) 
            GameSession.saludMaxima = int.Parse(inputSalud.text);
            
        if (!string.IsNullOrEmpty(inputReparaciones.text)) 
            GameSession.reparacionesMaximas = int.Parse(inputReparaciones.text);
            
        if (!string.IsNullOrEmpty(inputDanoCanon.text)) 
            GameSession.danoCanon = int.Parse(inputDanoCanon.text);
            
        if (!string.IsNullOrEmpty(inputDanoTorpedo.text)) 
            GameSession.danoTorpedo = int.Parse(inputDanoTorpedo.text);

        Debug.Log("Configuración Guardada. Tablero: " + GameSession.tamanoTablero);

        // 3. Cambiar de escena (A la de selección de jugadores o programación)
        SceneManager.LoadScene("Programacion"); 
    }
}