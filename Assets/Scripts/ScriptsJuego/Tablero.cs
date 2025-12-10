using UnityEngine;

public class Tablero : MonoBehaviour
{
    public static Tablero Instance; // Singleton para acceso f�cil

    [Header("Configuraci�n")]
    public int ancho = 10;
    public int alto = 10;
    public float tamañoCelda = 1.0f; // Cu�nto mide cada cuadro en Unity (metros)

    [Header("Visuales")]
    public GameObject prefabCasilla; // �Arrastra tu CasillaPrefab aqu�!

    // Enum para saber qu� hay en cada casilla
    public enum TipoCelda { Vacio, Pared, Barco, Mina, FueraDeLimites }

    // La matriz l�gica (El cerebro del mapa)
    private TipoCelda[,] grid;

    // Matriz para guardar REFERENCIAS a los objetos (para saber A QUI�N disparaste)
    private GameObject[,] objetosEnGrid;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- LEER CONFIGURACIÓN ---
        // Si tamanoTablero es 0 (ej. pruebas directas), usa el valor por defecto 10
        if (GameSession.tamanoTablero > 0)
        {
            ancho = GameSession.tamanoTablero;
            alto = GameSession.tamanoTablero;
        }
        // --------------------------

        InicializarTablero();
    }

    void InicializarTablero()
    {
        grid = new TipoCelda[ancho, alto];
        objetosEnGrid = new GameObject[ancho, alto];

        // --- GENERACI�N VISUAL DEL MAPA ---
        // Creamos un padre para que no se llene la jerarqu�a de objetos sueltos
        GameObject padreCeldas = new GameObject("Terreno_Agua");
        padreCeldas.transform.parent = this.transform;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // 1. Inicializar l�gica
                grid[x, y] = TipoCelda.Vacio;
                objetosEnGrid[x, y] = null;

                // 2. Crear visual (Si hay prefab asignado)
                if (prefabCasilla != null)
                {
                    // Calculamos posici�n (La Y la ponemos en -0.5 o 0 seg�n tu gusto)
                    // Si usas Quad, rota 90 en X. Si usas Plane, rotaci�n 0.
                    Vector3 pos = new Vector3(x * tamañoCelda, -0.05f, y * tamañoCelda);

                    GameObject celda = Instantiate(prefabCasilla, pos, Quaternion.identity);

                    celda.transform.rotation = Quaternion.identity;

                    celda.transform.parent = padreCeldas.transform;
                    celda.name = $"Celda_{x}_{y}";
                }
            }
        }
    }

    // --- CONSULTAS (Los barcos usar�n esto) ---

        // �Qu� hay en esta coordenada?
    public TipoCelda GetContenido(Vector2Int pos)
    {
        if (!EsCoordenadaValida(pos)) return TipoCelda.FueraDeLimites;
        return grid[pos.x, pos.y];
    }

    // �Es v�lido moverse aqu�? (Solo si es Vacio)
    public bool EsCaminable(Vector2Int pos)
    {
        if (!EsCoordenadaValida(pos)) return false;

        // Ahora permitimos caminar sobre VAC�O o sobre MINA (para que explote)
        TipoCelda contenido = grid[pos.x, pos.y];
        return contenido == TipoCelda.Vacio || contenido == TipoCelda.Mina;
    }

    // Validador de l�mites (0 a 9)
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
        // 1. Borramos la posici�n vieja
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

    // --- VISUALIZACI�N (Para que T� veas el grid en el editor) ---
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // Dibuja un cuadrito al�mbrico en cada celda
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