using UnityEngine;

public class Tablero : MonoBehaviour
{
    public static Tablero Instance; // Singleton para acceso fácil

    [Header("Configuración")]
    public int ancho = 10;
    public int alto = 10;
    public float tamañoCelda = 1.0f; // Cuánto mide cada cuadro en Unity (metros)

    [Header("Visuales")]
    public GameObject prefabCasilla; // ¡Arrastra tu CasillaPrefab aquí!

    // Enum para saber qué hay en cada casilla
    public enum TipoCelda { Vacio, Pared, Barco, Mina, FueraDeLimites }

    // La matriz lógica (El cerebro del mapa)
    private TipoCelda[,] grid;

    // Matriz para guardar REFERENCIAS a los objetos (para saber A QUIÉN disparaste)
    private GameObject[,] objetosEnGrid;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InicializarTablero();
    }

    void InicializarTablero()
    {
        grid = new TipoCelda[ancho, alto];
        objetosEnGrid = new GameObject[ancho, alto];

        // --- GENERACIÓN VISUAL DEL MAPA ---
        // Creamos un padre para que no se llene la jerarquía de objetos sueltos
        GameObject padreCeldas = new GameObject("Terreno_Agua");
        padreCeldas.transform.parent = this.transform;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // 1. Inicializar lógica
                grid[x, y] = TipoCelda.Vacio;
                objetosEnGrid[x, y] = null;

                // 2. Crear visual (Si hay prefab asignado)
                if (prefabCasilla != null)
                {
                    // Calculamos posición (La Y la ponemos en -0.5 o 0 según tu gusto)
                    // Si usas Quad, rota 90 en X. Si usas Plane, rotación 0.
                    Vector3 pos = new Vector3(x * tamañoCelda, -0.05f, y * tamañoCelda);

                    GameObject celda = Instantiate(prefabCasilla, pos, Quaternion.identity);

                    celda.transform.rotation = Quaternion.identity;

                    celda.transform.parent = padreCeldas.transform;
                    celda.name = $"Celda_{x}_{y}";
                }
            }
        }
    }

    // --- CONSULTAS (Los barcos usarán esto) ---

        // ¿Qué hay en esta coordenada?
    public TipoCelda GetContenido(Vector2Int pos)
    {
        if (!EsCoordenadaValida(pos)) return TipoCelda.FueraDeLimites;
        return grid[pos.x, pos.y];
    }

    // ¿Es válido moverse aquí? (Solo si es Vacio)
    public bool EsCaminable(Vector2Int pos)
    {
        if (!EsCoordenadaValida(pos)) return false;

        // Ahora permitimos caminar sobre VACÍO o sobre MINA (para que explote)
        TipoCelda contenido = grid[pos.x, pos.y];
        return contenido == TipoCelda.Vacio || contenido == TipoCelda.Mina;
    }

    // Validador de límites (0 a 9)
    public bool EsCoordenadaValida(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < ancho && pos.y >= 0 && pos.y < alto;
    }

    // --- ACCIONES (Mover y Colocar) ---

    public void RegistrarObjeto(Vector2Int pos, TipoCelda tipo, GameObject obj)
    {
        if (EsCoordenadaValida(pos))
        {
            grid[pos.x, pos.y] = tipo;
            objetosEnGrid[pos.x, pos.y] = obj;
        }
    }

    public void MoverObjeto(Vector2Int posAnterior, Vector2Int posNueva, TipoCelda tipo, GameObject obj)
    {
        // 1. Borramos la posición vieja
        if (EsCoordenadaValida(posAnterior))
        {
            grid[posAnterior.x, posAnterior.y] = TipoCelda.Vacio; // <--- ESTO LA DEJA LISTA PARA LA MINA
            objetosEnGrid[posAnterior.x, posAnterior.y] = null;
        }

        // 2. Ocupamos la nueva
        RegistrarObjeto(posNueva, tipo, obj);
    }

    public void EliminarObjeto(Vector2Int pos)
    {
        if (EsCoordenadaValida(pos))
        {
            grid[pos.x, pos.y] = TipoCelda.Vacio;
            objetosEnGrid[pos.x, pos.y] = null;
        }
    }

    // --- VISUALIZACIÓN (Para que TÚ veas el grid en el editor) ---
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // Dibuja un cuadrito alámbrico en cada celda
                Vector3 centro = new Vector3(x * tamañoCelda, 0, y * tamañoCelda);
                Gizmos.DrawWireCube(centro, new Vector3(tamañoCelda, 0.1f, tamañoCelda));
            }
        }
    }

    public GameObject GetObjetoEn(Vector2Int pos)
    {
        if (!EsCoordenadaValida(pos)) return null;
        return objetosEnGrid[pos.x, pos.y];
    }
}