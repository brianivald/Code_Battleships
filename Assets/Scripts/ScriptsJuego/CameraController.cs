using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;

    [Header("Configuración de UI")]
    [Range(0f, 0.5f)] public float margenIzquierdo = 0.05f;
    [Range(0f, 0.6f)] public float margenDerechoUI = 0.3f;
    [Range(0f, 0.5f)] public float margenSuperior = 0.1f;
    [Range(0f, 0.5f)] public float margenInferior = 0.1f;

    // Se ejecuta al iniciar el juego real
    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        AjustarCamaraAlTablero();
    }

    // Se ejecuta cuando mueves los sliders en el Editor
    void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        AjustarCamaraAlTablero();
    }

    void AjustarCamaraAlTablero()
    {
        // 1. Obtener datos de forma segura (para que no falle en el editor)
        int n = 10; // Valor por defecto
        float celda = 1f;

        // Si el juego está corriendo, usamos los datos reales
        if (Application.isPlaying)
        {
            n = GameSession.tamanoTablero > 0 ? GameSession.tamanoTablero : 10;
            if (Tablero.Instance != null) celda = Tablero.Instance.tamañoCelda;
        }
        else // Si estamos editando, buscamos el script manualmente para no dar error
        {
            Tablero tab = FindFirstObjectByType<Tablero>(); // Unity 2023+ (o FindObjectOfType)
            if (tab != null)
            {
                n = tab.ancho; // Usamos el ancho configurado en el inspector
                celda = tab.tamañoCelda;
            }
        }

        // 2. Calcular tamaño real
        float anchoMundo = n * celda;
        float altoMundo = n * celda;

        // 3. Calcular centro
        Vector3 centroTablero = new Vector3(anchoMundo / 2f - (celda / 2f), 0, altoMundo / 2f - (celda / 2f));

        // 4. Calcular área útil
        float anchoPantallaUtil = 1.0f - (margenIzquierdo + margenDerechoUI);
        float altoPantallaUtil = 1.0f - (margenSuperior + margenInferior);

        //Evitar división por cero si arrastras los sliders al máximo
        if (anchoPantallaUtil < 0.01f) anchoPantallaUtil = 0.01f;
        if (altoPantallaUtil < 0.01f) altoPantallaUtil = 0.01f;

        // 5. Calcular Zoom
        float aspectoPantalla = cam.aspect; // Usamos la propiedad directa de la cámara

        float sizeVertical = (altoMundo / altoPantallaUtil) / 2f;
        float sizeHorizontal = (anchoMundo / anchoPantallaUtil) / (2f * aspectoPantalla);

        cam.orthographicSize = Mathf.Max(sizeVertical, sizeHorizontal);

        // 6. Posicionar
        cam.transform.position = new Vector3(centroTablero.x, 10f, centroTablero.z);

        // Calcular desplazamiento en unidades de mundo
        float mundoAlto = 2f * cam.orthographicSize;
        float mundoAncho = mundoAlto * aspectoPantalla;

        float desplazamientoX = (margenDerechoUI - margenIzquierdo) / 2f * mundoAncho;
        float desplazamientoZ = (margenSuperior - margenInferior) / 2f * mundoAlto; // IMPORTANTE: Corregido signo

        cam.transform.position += new Vector3(desplazamientoX, 0, desplazamientoZ);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}