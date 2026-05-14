using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Unity.AI.Navigation;
using TMPro;
using Unity.VisualScripting;

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

        public bool target = false;
        public Vector2 targetCoords;

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
    public GridController grid;
    public Vector2 highlightedArea = new Vector2(0,0);
    private EnemySpawner enemySpawner;
    private TargetSpawner targetSpawner;

    private GameObject target;
    private int target_count = 0;

    public float fadeSpeed = 20f;
    public GameObject portalPrefab;

    private GameObject startPortal = null;

    public List<GameObject> PickUps = new List<GameObject>();

    public GameObject pauseMenu;
    public bool paused = false;

    private PlayerControls inputActions;
    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Awake()
    {
        inputActions = new PlayerControls();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        enemySpawner = GetComponent<EnemySpawner>();
        targetSpawner = GetComponent<TargetSpawner>();
    }

    void Start()
    {
        InitializeGrid();
        StartGame();
    }

    void Update()
    {
        if (inputActions.Player.Pause.WasPressedThisFrame())
        {
            if (!paused)
            {
                Pause();
            } else {
                UnPause();
            }
        }
    }

    void InitializeGrid()
    {
        Sector = new List<List<Area>>();
        int targetX = Random.Range(0, 4);
        int targetY = Random.Range(0, 4);

        Vector2 startCoord = FindFarthestArea(targetX, targetY);

        for (int x = 0; x < 4; x++)
        {
            List<Area> row = new List<Area>();

            for (int y = 0; y < 4; y++)
            {
                Area newArea = new Area(x, y, 0);

                GameObject areaObj = Instantiate(areaPrefab, Vector3.zero, Quaternion.identity);
                newArea.area = areaObj;

                AreaController areaController = areaObj.GetComponent<AreaController>();

                areaController.backgroundRocks.SpawnRocks(newArea);
                areaController.flightRocks.SpawnRocks(newArea);
                areaController.foregrounfRocks.SpawnRocks(newArea);

                if (x == targetX && y == targetY)
                {
                    TargetSpawner.Return r = targetSpawner.SpawnTarget(newArea.asteroids, newArea);
                    target = r.target;
                    target_count = r.num;
                    newArea.target = true;
                    newArea.targetCoords = r.coords;
                } 

                if (!(x == startCoord.x && y == startCoord.y))
                {
                    areaController.objectSpawner.SpawnObjects(newArea);
                    enemySpawner.SpawnEnemies(newArea);        
                }

                areaObj.SetActive(false);

                row.Add(newArea);
            }

            Sector.Add(row);
        }

        ActivateOnly((int)startCoord.x, (int)startCoord.y);

        Vector3 portalPos = new Vector3(player.position.x, -3.5f, player.position.z + 10f);
        Vector3 portalRot = new Vector3(0f, 90f, 0f);
        PortalController portal = Instantiate(portalPrefab, portalPos, Quaternion.Euler(portalRot)).GetComponent<PortalController>(); 
        portal.disabled = true;
        portal.player = player;
        startPortal = portal.gameObject;
    }

    public void ActivateOnly(int x, int y)
    {
        if (currArea != null)
        {
            foreach (AsteroidData asteroid in currArea.asteroids)
            {
                asteroid.rock.SetActive(false);
            }
            currArea.enemies = CullNullObjects(currArea.enemies);
            currArea.objects = CullNullObjects(currArea.objects);
        }

        foreach (GameObject pickup in PickUps)
        {
            Destroy(pickup);
        }
        PickUps.Clear();

        if (startPortal != null)
        {
            Destroy(startPortal);
            startPortal = null;
        }

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
            if (!enemy.GetComponent<ShipController>().dead) enemy.SetActive(true);
        }
        foreach (GameObject obj in currArea.objects)
        {
            obj.SetActive(true);
        }

        minimap.Clear();
        
        List<AsteroidData> filteredAsteroids = new();
        foreach (var asteroid in currArea.asteroids)
        {
            if (asteroid.rock.layer == 6 || asteroid.rock.layer == 12)
            {
                filteredAsteroids.Add(asteroid);
            }
        }

        minimap.DrawRocks(filteredAsteroids);        
        minimap.enemies = currArea.enemies;
        minimap.objects = currArea.objects;

        highlightedArea = new Vector2(x, y);
        GridController grid = minimap.GetComponentInChildren<GridController>();
        grid.ResetHighlights();
        grid.HighlightSquare();

        NavMeshSurface surface = currArea.area.GetComponentInChildren<NavMeshSurface>();
        surface.BuildNavMesh();
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

    Vector2 FindFarthestArea(int targetX, int targetY)
    {
        Vector2 farthest = new Vector2();
        int maxDistance = -1;

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                int distance = Mathf.Abs(x - targetX) + Mathf.Abs(y - targetY);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthest.x = x;
                    farthest.y = y;
                }
            }
        }
        return farthest;
    }

    public void UpdateTargets()
    {
        target_count -= 1;

        minimap.Clear();
        List<AsteroidData> filteredAsteroids = new();
        foreach (var asteroid in currArea.asteroids)
        {
            if (asteroid.rock.layer == 6 || asteroid.rock.layer == 12)
            {
                filteredAsteroids.Add(asteroid);
            }
        }
        minimap.DrawRocks(filteredAsteroids);  

        if (target_count <= 0)
        {
            target.SetActive(false);
            DeployExit();
        }
    }

    private void DeployExit()
    {
        List<Area> validAreas = new List<Area>();

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Area area = Sector[x][y];

                if (area == currArea)
                    continue;

                foreach (GameObject enemy in area.enemies)
                {
                    if (enemy != null)
                        enemy.GetComponent<ShipController>().dead = true;
                }

                enemySpawner.SpawnEnemies(area);

                int distance = Mathf.Abs(area.x_cord - currArea.x_cord) + Mathf.Abs(area.y_cord - currArea.y_cord);
                if (distance == 2) validAreas.Add(area);
            }
        }

        if (validAreas.Count == 0)
            return;

        Area portalArea = validAreas[Random.Range(0, validAreas.Count)];

        Vector3 portalPos = new Vector3(0f, -3.5f, 0f);
        Vector3 portalRot = new Vector3(0f, 90f, 0f);
        PortalController portal = Instantiate(portalPrefab, portalPos, Quaternion.Euler(portalRot)).GetComponent<PortalController>();

        portal.disabled = false;
        portal.player = player;

        portalArea.objects.Add(portal.gameObject);

        if (portalArea != currArea) portal.gameObject.SetActive(false);

        grid.SetPortal(portalArea.x_cord, portalArea.y_cord);
        minimap.SetPortalCoords(new Vector2(portalArea.x_cord, portalArea.y_cord));
    }

    public void StartGame()
    {
        PlayerController playerController = player.GetComponentInParent<PlayerController>();
        playerController.Startup(fadeSpeed);        
    }

    public void FinishGame()
    {
        minimap.gameObject.SetActive(false);
        PlayerController playerController = player.GetComponentInParent<PlayerController>();
        playerController.Shutdown(fadeSpeed);
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        PlayerController playerController = player.GetComponentInParent<PlayerController>();
        if (playerController != null) playerController.enabled = false;
        pauseMenu.SetActive(true);
    }

    public void UnPause()
    {
        paused = false;
        Time.timeScale = 1f;
        PlayerController playerController = player.GetComponentInParent<PlayerController>();
        if (playerController != null) playerController.enabled = true;
        pauseMenu.SetActive(false);
    }
}