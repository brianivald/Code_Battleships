using UnityEngine;
using UnityEngine.SceneManagement; // �IMPORTANTE! Sin esto no funciona

public class NavegacionManager : MonoBehaviour
{
    // Funci�n para ir a la pantalla de Programaci�n
    public void IrAProgramacion()
    {
        // Aseg�rate de que el nombre entre comillas sea EXACTO al de tu escena
        SceneManager.LoadScene("Programacion");
    }

    // Funci�n para ir a la pantalla de Configuraci�n
    public void IrAConfiguracion()
    {
        SceneManager.LoadScene("Configuracion");
    }

    public void IrAMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}