using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // ¡Necesario para List!

public class BoardManager : MonoBehaviour
{
    [Header("Prefabs y Paneles")]
    public GameObject cellPrefab;
    public GameObject barcoPrefab;
    public Transform gridPanel;

    [Header("Tamaño del Tablero")]
    public int filas = 10;
    public int columnas = 10;

    private GameObject[,] celdas;
    // MEJORA 1: Lista para rastrear posiciones ya usadas
    private List<Vector2Int> posicionesOcupadas = new List<Vector2Int>();

    void Start()
    {
        GenerarTablero();
        ColocarBarcos();
    }

    // ============================================================
    // 1. Generar las celdas del tablero (sin cambios)
    // ============================================================
    void GenerarTablero()
    {
        celdas = new GameObject[columnas, filas]; // NOTA: Usar [columna, fila] es más intuitivo
                                                  // Pero mantenemos tu [filas, columnas] por ahora

        // Asumo que quieres filas=X y columnas=Y
        for (int x = 0; x < columnas; x++)
        {
            for (int y = 0; y < filas; y++)
            {
                GameObject celda = Instantiate(cellPrefab, gridPanel);
                celda.name = $"Celda_{x}_{y}";

                celda.GetComponent<Image>().color = new Color(0.75f, 0.58f, 0.40f);

                celdas[x, y] = celda;
            }
        }
    }

    // ============================================================
    // 2. Colocar barcos dentro de celdas coloreadas (con la mejora)
    // ============================================================
    void ColocarBarcos()
    {
        // ... (Tu código de colores)
        Color[] colores = new Color[4]
        {
            new Color(1f, 0.55f, 0f),      // Naranja
            new Color(0.5f, 0f, 0.8f),     // Morado
            new Color(0f, 0.7f, 0.2f),     // Verde
            new Color(1f, 0.4f, 0.7f)      // Rosa
        };

        for (int i = 0; i < 4; i++)
        {
            // MEJORA 2: Llamamos a la función que garantiza la unicidad
            Vector2Int pos = GenerarPosicionUnica();

            // Usamos las coordenadas para obtener la celda
            GameObject celdaGO = celdas[pos.x, pos.y];

            // ... (Tu código de instanciación del barco)
            celdaGO.GetComponent<Image>().color = colores[i];

            GameObject barco = Instantiate(barcoPrefab);
            RectTransform rt = barco.GetComponent<RectTransform>();

            rt.SetParent(celdaGO.transform, false);

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            Image imgBarco = barco.GetComponent<Image>();
            if (imgBarco != null)
                imgBarco.color = Color.white;

            barco.transform.SetAsLastSibling();
        }
    }

    // ============================================================
    // 3. Posición Aleatoria Única (NUEVA FUNCIÓN)
    // ============================================================
    Vector2Int GenerarPosicionUnica()
    {
        // Esta es una medida de seguridad, si ya no quedan espacios libres
        if (posicionesOcupadas.Count >= columnas * filas)
        {
            Debug.LogError("Error: No hay más celdas libres para colocar barcos.");
            return new Vector2Int(-1, -1);
        }

        while (true)
        {
            // Generamos coordenadas aleatorias dentro de los límites del tablero
            int x = Random.Range(0, columnas);
            int y = Random.Range(0, filas);

            Vector2Int p = new Vector2Int(x, y);

            if (!posicionesOcupadas.Contains(p))
            {
                posicionesOcupadas.Add(p);
                return p;
            }
        }
    }
}
