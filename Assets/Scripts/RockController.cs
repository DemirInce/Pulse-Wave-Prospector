using UnityEngine;

public class RockController : MonoBehaviour
{
    [Header("Spin Settings")]
    public float minSpinRate = 2f;
    public float maxSpinRate = 9f;

    private float spinRate;
    private Vector3 spinAxis;

    void Start()
    {
        spinRate = Random.Range(minSpinRate, maxSpinRate);
        spinAxis = Random.onUnitSphere;
    }

    void Update()
    {
        transform.Rotate(spinAxis * spinRate * Time.deltaTime);
    }
}