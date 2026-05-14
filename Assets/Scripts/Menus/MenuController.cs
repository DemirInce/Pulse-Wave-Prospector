using UnityEngine;

public class MenuController : MonoBehaviour
{

    public RockSpawner backgroundRocks;

    void Start()
    {
        backgroundRocks.SpawnRocks(null);
    }
}
