/*using UnityEngine;


public class GeneradorBarcos : MonoBehaviour
{
    public GameObject[] barcosPrefabs;
    //public GameObject barcoPrefab;    // arrastra aqui Barco.prefab
    public Transform boardParent;     // arrastra aqui el objeto Board (con Grid Layout)
    public int filas = 8;
    public int columnas = 8;

    // posiciones de ejemplo: {fila, columna}
    // puedes cambiarlas o generar aleatoriamente más adelante
    public Vector2Int[] posicionesBarcos = new Vector2Int[4]
    {
        new Vector2Int(0, 0), // esquina superior izquierda
        new Vector2Int(2, 3),
        new Vector2Int(5, 1),
        new Vector2Int(7, 6)
    };

    void Start()
    {
       if (barcoPrefab == null || boardParent == null)
        {
            Debug.LogError("Asigna barcoPrefab y boardParent en el Inspector.");
            return;
        }

        GenerarBarcos();
    }

    void GenerarBarcos()
    {
        // El Grid coloca los hijos en el orden de sibling index.
        // Para colocar un barco en (fila,col) calculamos el index = fila * columnas + columna
        foreach (Vector2Int pos in posicionesBarcos)
        {
            int index = pos.x * columnas + pos.y;

            // Instanciar como hijo del boardParent (no mantiene posición mundial)
            GameObject go = Instantiate(barcoPrefab, boardParent);

            // Opcional: renombrar para identificar
            go.name = $"Barco_{pos.x}_{pos.y}";

            // Colocar en el siblingIndex correcto para que el Grid lo posicione en esa celda
            // Clamp index entre 0 y (filas*columnas - 1)
            int maxIndex = filas * columnas - 1;
            int idx = Mathf.Clamp(index, 0, maxIndex);
            go.transform.SetSiblingIndex(idx);
        }
    }
}*/
using UnityEngine;


public class GeneradorBarcos : MonoBehaviour
{
    
    public GameObject[] barcosPrefabs;   // ← AQUI ESTARÁN los 4 PREFABS
    public Transform boardParent;        // grid del tablero
    public int filas = 8;
    public int columnas = 8;

    // posiciones donde pondremos 4 barcos
    public Vector2Int[] posicionesBarcos = new Vector2Int[4]
    {
        new Vector2Int(0, 0),
        new Vector2Int(2, 3),
        new Vector2Int(5, 1),
        new Vector2Int(7, 6)
    };

    void Start()
    {
        if (barcosPrefabs == null || barcosPrefabs.Length < 4)
        {
            Debug.LogError("Debes asignar 4 prefabs en barcosPrefabs (Size 4)");
            return;
        }

        if (boardParent == null)
        {
            Debug.LogError("boardParent no está asignado.");
            return;
        }

        GenerarBarcos();
    }

    void GenerarBarcos()
    {
        for (int i = 0; i < posicionesBarcos.Length; i++)
        {
            int index = posicionesBarcos[i].x * columnas + posicionesBarcos[i].y;
            int maxIndex = filas * columnas - 1;
            index = Mathf.Clamp(index, 0, maxIndex);

            // CREA CADA BARCO CON SU PREFAB
            GameObject go = Instantiate(barcosPrefabs[i], boardParent);

            go.name = $"Barco_{posicionesBarcos[i].x}_{posicionesBarcos[i].y}";

            // Colocar barco en la celda correspondiente del Grid
            go.transform.SetSiblingIndex(index);
        }
    }
}
