using UnityEngine;
using UnityEngine.UI;

public class BoardGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public int rows = 8;
    public int columns = 8;
    public float cellSize = 64f;

    public Color color1 = new Color(0.8f, 0.8f, 0.8f); // gris claro
    public Color color2 = new Color(0.6f, 0.6f, 0.6f); // gris oscuro

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                RectTransform rect = cell.GetComponent<RectTransform>();

                rect.anchoredPosition = new Vector2(
                    col * cellSize,
                    -row * cellSize
                );

                // Alternar colores tipo ajedrez
                Image img = cell.GetComponent<Image>();
                if ((row + col) % 2 == 0)
                    img.color = color1;
                else
                    img.color = color2;
            }
        }
    }
}
