using System.Collections.Generic;
using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Particle Effects")]
    public List<ParticleSystem> effects;

    private List<Transform> availablePoints;

    void Awake()
    {
        availablePoints = new List<Transform>(spawnPoints);
    }

    public void SpawnEffect()
    {
        if (availablePoints.Count == 0)
        {
            Debug.Log("No more spawn points available.");
            return;
        }

        int pointIndex = Random.Range(0, availablePoints.Count);
        Transform chosenPoint = availablePoints[pointIndex];

        availablePoints.RemoveAt(pointIndex);

        int effectIndex = Random.Range(0, effects.Count);
        ParticleSystem chosenEffect = effects[effectIndex];

        ParticleSystem instance = Instantiate(
            chosenEffect,
            chosenPoint.position,
            Quaternion.identity
        );

        instance.transform.SetParent(chosenPoint, true);
    }

    public void ResetPoints()
    {
        availablePoints = new List<Transform>(spawnPoints);
    }
}