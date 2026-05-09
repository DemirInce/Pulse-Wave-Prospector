using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridController : MonoBehaviour
{
    [Header("References")]
    public RectTransform container;
    public GameObject squarePrefab;
    public GameObject linePrefab;

    [Header("Grid Settings")]
    public int gridSize = 4;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
    public Color defaultColor = new Color(0f, 1f, 0f, 0.1f);

    private List<Image> squares = new List<Image>();

    void Start()
    {
        DrawGrid();
        HighlightSquare(0, 0);
    }

    void DrawGrid()
    {
        float width = container.rect.width;
        float height = container.rect.height;

        float cellWidth = width / gridSize;
        float cellHeight = height / gridSize;

        // --- Create Squares ---
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject square = Instantiate(squarePrefab, container);
                square.transform.SetAsFirstSibling(); // send behind other UI

                RectTransform rt = square.GetComponent<RectTransform>();
                Image img = square.GetComponent<Image>();

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.sizeDelta = new Vector2(cellWidth, cellHeight);

                float x = col * cellWidth + cellWidth / 2f;
                float y = row * cellHeight + cellHeight / 2f;

                rt.anchoredPosition = new Vector2(x, y);

                squares.Add(img);
            }
        }

        // --- Vertical Lines ---
        for (int i = 1; i < gridSize; i++)
        {
            GameObject line = Instantiate(linePrefab, container);
            line.transform.SetAsFirstSibling();

            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.sizeDelta = new Vector2(2f, height);

            float x = i * cellWidth;
            rt.anchoredPosition = new Vector2(x, height / 2f);
        }

        // --- Horizontal Lines ---
        for (int i = 1; i < gridSize; i++)
        {
            GameObject line = Instantiate(linePrefab, container);
            line.transform.SetAsFirstSibling();

            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.sizeDelta = new Vector2(width, 2f);

            float y = i * cellHeight;
            rt.anchoredPosition = new Vector2(width / 2f, y);
        }
    }

    public void HighlightSquare(int row, int col)
    {
        int index = col * gridSize + row;

        if (index < 0 || index >= squares.Count)
        {
//            Debug.LogWarning("Invalid square index");
            return;
        }

        squares[index].color = highlightColor;
    }

    public void ResetHighlights()
    {
        foreach (var img in squares)
        {
            img.color = defaultColor;
        }
    }
}