using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridController : MonoBehaviour
{
    [Header("References")]
    public RectTransform container;
    public GameObject squarePrefab;
    public GameObject linePrefab;
    public GameObject shipDot;
    public GameObject portalDot;

    [Header("Grid Settings")]
    public int gridSize = 4;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
    public Color defaultColor = new Color(0f, 1f, 0f, 0.1f);

    private List<Image> squares = new List<Image>();
    private List<GameObject> shipDots = new List<GameObject>();
    private List<GameObject> portalDots = new List<GameObject>();

    void Start()
    {
        DrawGrid();
        HighlightSquare();
    }

    void DrawGrid()
    {
        float width = container.rect.width;
        float height = container.rect.height;

        float cellWidth = width / gridSize;
        float cellHeight = height / gridSize;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject square = Instantiate(squarePrefab, container);
                square.transform.SetAsFirstSibling();

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

                GameObject dot = Instantiate(shipDot, container);
                dot.GetComponent<Image>().color = Color.blue;

                RectTransform dotRT = dot.GetComponent<RectTransform>();

                dotRT.anchorMin = Vector2.zero;
                dotRT.anchorMax = Vector2.zero;
                dotRT.pivot = new Vector2(0.5f, 0.5f);

                dotRT.localRotation = Quaternion.Euler(0f, 0f, 90f);

                dotRT.anchoredPosition = new Vector2(x, y);

                dot.SetActive(false);

                shipDots.Add(dot);

                GameObject portal = Instantiate(portalDot, container);

                RectTransform portalRT = portal.GetComponent<RectTransform>();

                portalRT.anchorMin = Vector2.zero;
                portalRT.anchorMax = Vector2.zero;
                portalRT.pivot = new Vector2(0.5f, 0.5f);

                portalRT.anchoredPosition = new Vector2(x, y);

                portal.SetActive(false);

                portalDots.Add(portal);
            }
        }

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

    public void HighlightSquare()
    {
        ResetHighlights();

        var gm = GameManager.Instance;

        int col = (int)gm.highlightedArea.y;
        int row = (int)gm.highlightedArea.x;

        int index = col * gridSize + row;

        if (index < 0 || index >= squares.Count)
        {
            return;
        }

        squares[index].color = highlightColor;

        if (index >= 0 && index < portalDots.Count)
        {
            portalDots[index].SetActive(false);
        }

        shipDots[index].SetActive(true);
    }

    public void ResetHighlights()
    {
        foreach (var img in squares)
        {
            img.color = defaultColor;
        }

        foreach (var dot in shipDots)
        {
            dot.SetActive(false);
        }
    }

    public void SetPortal(int x, int y)
    {
        foreach (var dot in portalDots)
        {
            dot.SetActive(false);
        }

        int col = y;
        int row = x;

        int index = col * gridSize + row;

        if (index < 0 || index >= portalDots.Count)
        {
            return;
        }

        var gm = GameManager.Instance;

        bool playerHere =
            (int)gm.highlightedArea.x == x &&
            (int)gm.highlightedArea.y == y;

        if (!playerHere)
        {
            portalDots[index].SetActive(true);
        }
    }
}