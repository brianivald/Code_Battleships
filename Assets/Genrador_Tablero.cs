using UnityEngine;

public class GeneradorTablero : MonoBehaviour
{
    public GameObject prefabCasilla;
    public int tamañoTablero = 10;

    void Start()
    {
        if (prefabCasilla == null)
        {
            Debug.LogError("Asigna el prefab de la casilla en el Inspector!");
            return;
        }

        GenerarTablero();
    }

    void GenerarTablero()
    {
        // Limpiar tablero existente
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generar nuevo tablero
        for (int x = 0; x < tamañoTablero; x++)
        {
            for (int y = 0; y < tamañoTablero; y++)
            {
                Vector3 posicion = new Vector3(x, y, 0);
                GameObject casilla = Instantiate(prefabCasilla, posicion, Quaternion.identity);
                casilla.transform.SetParent(this.transform);
                casilla.name = $"Casilla_{x}_{y}";
            }
        }

        Debug.Log($"Tablero generado: {tamañoTablero}x{tamañoTablero}");
    }
}