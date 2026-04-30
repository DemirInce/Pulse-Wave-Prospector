using System.Collections.Generic;
using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Particle Effects")]
    public List<ParticleSystem> effects;

    private List<Transform> availablePoints;
    private Dictionary<Transform, ParticleSystem> activeEffects;

    void Awake()
    {
        availablePoints = new List<Transform>(spawnPoints);
        activeEffects = new Dictionary<Transform, ParticleSystem>();
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

        activeEffects[chosenPoint] = instance;
    }

    public void RemoveEffect()
    {
        if (activeEffects.Count == 0)
        {
            Debug.Log("No active effects to remove.");
            return;
        }

        var keys = new List<Transform>(activeEffects.Keys);
        int index = Random.Range(0, keys.Count);
        Transform point = keys[index];

        ParticleSystem effect = activeEffects[point];

        if (effect != null)
        {
            Destroy(effect.gameObject);
        }

        activeEffects.Remove(point);

        availablePoints.Add(point);
    }
}