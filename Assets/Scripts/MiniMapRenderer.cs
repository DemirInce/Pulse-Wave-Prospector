using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapRenderer : MonoBehaviour
{
    public RectTransform mapArea;
    public GameObject rockDot;
    public GameObject shipDot;

    [Header("World Mapping")]
    public Transform mapOrigin;
    public float worldWidth = 100f;
    public float worldLength = 100f;

    private List<GameObject> dots = new List<GameObject>();
    public List<GameObject> enemies;
    public List<GameObject> objects;
    private GameObject playerDot = null;

    public void DrawRocks(List<GameManager.AsteroidData> asteroids)
    {
        foreach (var asteroid in asteroids) { 
            CreateDot(WorldToMap(asteroid.position), Color.gray, rockDot, scale: asteroid.radius*0.2f); 
        }     
    }
    
    public void UpdateScan(Vector3 playerPos)
    {
        if (playerDot == null)
        {
            playerDot = Instantiate(shipDot, mapArea);
            playerDot.GetComponent<Image>().color = Color.blue;
        }

        RectTransform rt = playerDot.GetComponent<RectTransform>();
        rt.anchoredPosition = WorldToMap(playerPos);

        float angle = DirectionToAngle(GameManager.Instance.player.forward);
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void DrawScan()
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float angle = DirectionToAngle(enemy.transform.forward);

            CreateDot(
                WorldToMap(enemy.transform.position),
                Color.red,
                shipDot,
                fade: true,
                angle: angle
            );
        }

        foreach (var obj in objects)
        {
            if (obj == null || !obj.activeInHierarchy) continue;
            CreateDot(
                WorldToMap(obj.transform.position),
                Color.orange,
                rockDot,
                fade: true,
                scale: 1.3f
            );            
        }
    }

    Vector2 WorldToMap(Vector3 worldPos)
    {
        Vector3 diff = worldPos - mapOrigin.position;

        float x = diff.x / worldWidth;
        float y = diff.z / worldLength;

        float mapSize = mapArea.rect.width;

        return new Vector2(x, y) * mapSize;
    }

    float DirectionToAngle(Vector3 forward)
    {
        Vector2 dir = new Vector2(forward.x, forward.z).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return angle;
    }

    void CreateDot(Vector2 pos, Color color, GameObject dotPrefab, bool fade = false, float angle = 0f, float scale = 1f)
    {
        GameObject dot = Instantiate(dotPrefab, mapArea);
        dot.GetComponent<Image>().color = color;

        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        rt.localRotation = Quaternion.Euler(0, 0, angle);
        
        rt.localScale *= scale;

        if (fade)
            dot.AddComponent<DotFade>();

        dots.Add(dot);
    }

    public void Clear()
    {
        foreach (var d in dots)
            Destroy(d);

        dots.Clear();
    }
}