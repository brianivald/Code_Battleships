using UnityEngine;
using TMPro; // ¡Importante! Necesitas esto para trabajar con TextMeshPro

// Esto asegura que el script solo se pueda poner en objetos que tengan un TMP_InputField
[RequireComponent(typeof(TMP_InputField))]
public class limite_entradas : MonoBehaviour
{
    // --- Variables Públicas ---
    // Estas aparecerán en el Inspector de Unity
    public int minLimit = 0;
    public int maxLimit = 999;
    // ---

    private TMP_InputField inputField;

    // "Awake" se llama una vez, justo al inicio (antes que "Start")
    void Awake()
    {
        // 1. Obtenemos el componente InputField que está en este mismo objeto
        inputField = GetComponent<TMP_InputField>();

        // 2. Nos aseguramos de que el campo sea de tipo "Integer Number"
        // (Aunque ya lo hayas puesto, es una buena práctica)
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // 3. ¡La parte clave!
        // Le decimos al InputField: "Oye, cuando el usuario termine de editar,
        // llama a mi método 'ValidateInput'".
        inputField.onEndEdit.AddListener(ValidateInput);
    }

    // Este método se llamará automáticamente gracias a la línea de arriba
    public void ValidateInput(string text)
    {
        // Si el campo está vacío, lo reseteamos al valor mínimo
        if (string.IsNullOrEmpty(text))
        {
            inputField.text = minLimit.ToString();
            return;
        }

        int currentValue;

        // Intentamos convertir el texto a un número entero
        if (int.TryParse(text, out currentValue))
        {
            // Es un número válido. Ahora lo forzamos a estar en el rango.
            // "Mathf.Clamp" toma un valor y lo "aplasta" entre un mínimo y un máximo.
            int clampedValue = Mathf.Clamp(currentValue, minLimit, maxLimit);

            // Actualizamos el texto del campo SOLO si el valor fue cambiado.
            // (Esto evita bucles infinitos de actualización)
            if (clampedValue != currentValue)
            {
                inputField.text = clampedValue.ToString();
            }
        }
        else
        {
            // El usuario escribió algo inválido (ej. solo un "-")
            // Lo forzamos al valor mínimo.
            inputField.text = minLimit.ToString();
        }
    }

    // Es buena práctica limpiar el "listener" cuando el objeto se destruye
    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(ValidateInput);
        }
    }
}