using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Heatmap : MonoBehaviour
{
    [Header("Overlay")]
    public RawImage minimapImage;

    [Header("Texture")]
    public int textureResolution = 128;

    [Header("Heat Gradient")]
    public Gradient heatGradient;

    [Header("Fade")]
    public float fadeDuration = 5f;

    [Header("Heat")]
    public float heatStrength = 1f;

    private const int AREA_SIZE = 300;
    private const int GRID_SIZE = 4;
    private const int WORLD_SIZE = AREA_SIZE * GRID_SIZE;

    private Coroutine fadeRoutine;

    public void GenerateAndDisplayHeatmap(
        List<List<GameManager.Area>> areas,
        GameManager.Area area)
    {
        Texture2D heatmap =
            GenerateHeatMap(areas, area, textureResolution);

        minimapImage.texture = heatmap;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeHeatmap());
    }

    IEnumerator FadeHeatmap()
    {
        float t = 0f;

        Color c = minimapImage.color;
        c.a = 1f;
        minimapImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float alpha =
                Mathf.Lerp(1f, 0f, t / fadeDuration);

            c.a = alpha;

            minimapImage.color = c;

            yield return null;
        }

        c.a = 0f;
        minimapImage.color = c;
    }

    private Vector2 GetTargetGlobalPosition(
        List<List<GameManager.Area>> areas)
    {
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                GameManager.Area area = areas[x][y];

                if (area.target)
                {
                    float areaCenterX =
                        x * AREA_SIZE + AREA_SIZE * 0.5f;

                    float areaCenterY =
                        y * AREA_SIZE + AREA_SIZE * 0.5f;

                    return new Vector2(
                        areaCenterX + area.targetCoords.x,
                        areaCenterY + area.targetCoords.y
                    );
                }
            }
        }

        return Vector2.zero;
    }

    private Texture2D GenerateHeatMap(
        List<List<GameManager.Area>> areas,
        GameManager.Area area,
        int resolution)
    {
        Texture2D tex = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 targetPos =
            GetTargetGlobalPosition(areas);

        float areaOriginX =
            area.x_cord * AREA_SIZE;

        float areaOriginY =
            area.y_cord * AREA_SIZE;

        for (int py = 0; py < resolution; py++)
        {
            for (int px = 0; px < resolution; px++)
            {
                float localX =
                    ((float)px / (resolution - 1))
                    * AREA_SIZE;

                float localY =
                    ((float)py / (resolution - 1))
                    * AREA_SIZE;

                float worldX =
                    areaOriginX + localX;

                float worldY =
                    areaOriginY + localY;

                Vector2 pixelWorldPos =
                    new Vector2(worldX, worldY);

                float dist =
                    Vector2.Distance(
                        pixelWorldPos,
                        targetPos);

                float heat =
                    1f -
                    Mathf.Clamp01(
                        dist / WORLD_SIZE);

                heat *= heatStrength;

                Color color =
                    heatGradient.Evaluate(heat);

                // IMPORTANT
                // Makes distant areas transparent
                color.a = heat * 0.8f;

                tex.SetPixel(px, py, color);
            }
        }

        tex.Apply();

        return tex;
    }
}