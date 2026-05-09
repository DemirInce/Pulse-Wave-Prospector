using UnityEngine;
using UnityEngine.UI;

public class DotFade : MonoBehaviour
{
    public float lifetime = 5f;

    private float timer;
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifetime;

        Color c = img.color;
        c.a = 1f - t;
        img.color = c;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}