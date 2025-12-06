using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private Vector3 destino;
    private float velocidad = 10f;
    private bool activo = false;

    public void Configurar(Vector3 _destino)
    {
        destino = _destino;
        activo = true;
    }

    void Update()
    {
        if (!activo) return;

        // Moverse hacia el destino
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

        // Si llega (o está muy cerca), se destruye
        if (Vector3.Distance(transform.position, destino) < 0.1f)
        {
            // Aquí podrías instanciar una partícula de explosión
            Destroy(gameObject);
        }
    }
}