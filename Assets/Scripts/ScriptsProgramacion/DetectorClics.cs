using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem; // <--- 1. AÑADIR ESTO

public class DetectorClics : MonoBehaviour, IPointerClickHandler
{
    public LogicBuilder logicBuilder;
    private TextMeshProUGUI textoTMP;

    void Start()
    {
        textoTMP = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 2. CAMBIAR 'Input.mousePosition' POR ESTO:
        Vector3 mousePos = Mouse.current.position.ReadValue();

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textoTMP, mousePos, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textoTMP.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();
            int indiceFila = int.Parse(linkId);
            logicBuilder.SeleccionarFila(indiceFila);
        }
        else
        {
            logicBuilder.SeleccionarFila(-1);
        }
    }
}