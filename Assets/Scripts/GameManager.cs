using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Unity.AI.Navigation;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [System.Serializable]
    public class AsteroidData
    {
        public Vector3 position;
        public float radius;
        public GameObject rock;

        public AsteroidData(Vector3 pos, float r, GameObject rockObejct)
        {
            position = pos;
            radius = r;
            rock = rockObejct;
        }
    }

    [System.Serializable]
    public class Area
    {
        public int x_cord;
        public int y_cord;
        public int difficulty;

        public GameObject area;
        public List<AsteroidData> asteroids = new List<AsteroidData>(); 
        public List<GameObject> enemies = new List<GameObject>();
        public List<GameObject> objects = new List<GameObject>();

        public Area(int x, int y, int diff)
        {
            x_cord = x;
            y_cord = y;
            difficulty = diff;
        }
    }
    public Transform player;

    public List<List<Area>> Sector;
    public Area currArea;
    public GameObject areaPrefab;

    public MiniMapRenderer minimap;
    private EnemySpawner spawner;
    private TargetSpawner targetSpawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        spawner = GetComponent<EnemySpawner>();
        targetSpawner = GetComponent<TargetSpawner>();
    }

    void Start()
    {
        InitializeGrid();
        ActivateOnly(0,0);
    }

    void InitializeGrid()
    {
        Sector = new List<List<Area>>();

        for (int x = 0; x < 4; x++)
        {
            List<Area> row = new List<Area>();

            for (int y = 0; y < 4; y++)
            {
                Area newArea = new Area(x, y, 0);
                currArea = newArea;

                GameObject areaObj = Instantiate(areaPrefab, new Vector3(0f,0f,0f), Quaternion.identity);
                newArea.area = areaObj;

                spawner.SpawnEnemies(newArea);

                targetSpawner.SpawnTarget(newArea.asteroids);

                areaObj.SetActive(x == 0 && y == 0);

                row.Add(newArea);
            }

            Sector.Add(row);
        }
    }

    public void ActivateOnly(int x, int y)
    {
        foreach (AsteroidData asteroid in currArea.asteroids)
        {
            asteroid.rock.SetActive(false);
        }
        currArea.enemies = CullNullObjects(currArea.enemies);
        currArea.objects = CullNullObjects(currArea.objects);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                bool isTarget = (i == x && j == y);
                Sector[i][j].area.SetActive(isTarget);

                if (isTarget)
                    currArea = Sector[i][j];
            }
        }

        foreach (AsteroidData asteroid in currArea.asteroids)
        {
            asteroid.rock.SetActive(true);
        }
        foreach (GameObject enemy in currArea.enemies)
        {
            enemy.SetActive(true);
        }
        foreach (GameObject obj in currArea.objects)
        {
            obj.SetActive(true);
        }

        minimap.Clear();
        
        List<AsteroidData> filteredAsteroids = new();
        foreach (var asteroid in currArea.asteroids)
        {
            if (asteroid.rock.layer == 6)
            {
                filteredAsteroids.Add(asteroid);
            }
        }

        minimap.DrawRocks(filteredAsteroids);        
        minimap.enemies = currArea.enemies;
        minimap.objects = currArea.objects;

        GridController grid = minimap.GetComponentInChildren<GridController>();
        grid.ResetHighlights();
        grid.HighlightSquare(x,y);

        NavMeshSurface surface = currArea.area.GetComponentInChildren<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    public void RegisterAsteroid(Vector3 pos, float radius, GameObject rock)
    {
        currArea.asteroids.Add(new AsteroidData(pos, radius, rock));
    }

    List<GameObject> CullNullObjects(List<GameObject> objects)
    {
        List<GameObject> newObjects = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (!obj.activeInHierarchy)
            {
                Destroy(obj);
            } else
            {
                obj.SetActive(false);
                newObjects.Add(obj);
            }
        }
        return newObjects;
    }
}