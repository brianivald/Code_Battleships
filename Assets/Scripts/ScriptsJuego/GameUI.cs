using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelGameOver;
    public TextMeshProUGUI textoGanador;

    public void MostrarVictoria(string nombreGanador, Color colorGanador)
    {
        panelGameOver.SetActive(true);
        textoGanador.text = "�Ganador: " + nombreGanador + "!";
        textoGanador.color = colorGanador;
    }

    public void IrAlMenu()
    {
        // Aseg�rate de usar el nombre correcto de tu escena de men�
        SceneManager.LoadScene("MainMenu");
    }
    public void OcultarPanel()
    {
        panelGameOver.SetActive(false);
    }
}