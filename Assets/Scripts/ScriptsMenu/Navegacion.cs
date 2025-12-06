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
}
