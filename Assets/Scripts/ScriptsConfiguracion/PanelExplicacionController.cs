using UnityEngine;
using TMPro;

public class PanelExplicacionController : MonoBehaviour
{
    public GameObject panelExplicacion;
    public TextMeshProUGUI textoExplicacion;

    public void MostrarExplicacion(string mensaje)
    {
        textoExplicacion.text = mensaje;
        panelExplicacion.SetActive(true);
    }

    public void Cerrar()
    {
        panelExplicacion.SetActive(false);
    }
    
}
