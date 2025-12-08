using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Ir a una escena específica por nombre
    public void CargarTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Configuracion");
    }
    // En GameUI.cs

    public void SalirDelJuego()
    {
        Debug.Log("Solicitud de salir del juego recibida."); // Para que veas que funciona en el Editor
        Application.Quit(); // Esto cierra la ventana del juego real
    }
}
