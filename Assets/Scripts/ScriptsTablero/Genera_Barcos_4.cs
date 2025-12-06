using UnityEngine;

public class Genera_Barcos_4 : MonoBehaviour
{
    [Header("Prefabs de Barcos (Asigna 4 prefabs)")]
    public GameObject[] barcosPrefabs;

    [Header("Cantidad de barcos a generar")]
    public int cantidadDeBarcos = 4;

    [Header("Contenedor donde se guardarán en la jerarquía")]
    public Transform contenedor;

    void Start()
    {
        GenerarBarcos();
    }

    void GenerarBarcos()
    {
        for (int i = 0; i < cantidadDeBarcos; i++)
        {
            Instantiate(
                barcosPrefabs[i],            // Prefab del barco
                ObtenerPosicionAleatoria(),  // Posición aleatoria
                Quaternion.identity,         // Sin rotación
                contenedor                   // Para organización
            );
        }
    }

    Vector3 ObtenerPosicionAleatoria()
    {
        float x = Random.Range(-4f, 4f);
        float y = Random.Range(-4f, 4f);
        return new Vector3(x, y, 0);
    }
}

