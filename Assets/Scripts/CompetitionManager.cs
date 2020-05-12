using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CompetitionManager : MonoBehaviour
{
    public static CompetitionManager current;
    
    public bool gameStarted = false;
    public bool gameOver = false;
    [Range(10, 30)] public float powerupsInterval = 10;
    [Range(20, 50)] public float powerupSpawnRadius = 20;

    public Vector3 mapCenter = Vector3.zero;
    public Vector3 mapSize = new Vector3(1000, 0, 1000);

    public int ShipsLeft => _ships.Count;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] shipPrefabs;
    [SerializeField] private GameObject[] powerupPrefabs;
    [SerializeField] private Transform powerupSpawnCenter;

    private List<Ship> _ships = new List<Ship>();
    
    private void Awake()
    {
        current = this;
        
        SpawnShips();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!gameStarted)
            {
                gameStarted = true;
                foreach (Ship ship in _ships) StartCoroutine(ship.RunAI(this));
                StartCoroutine(SpawnPowerup());
            }
        }
    }

    private void SpawnShips()
    {
        int num = Mathf.Min(spawnPoints.Length, shipPrefabs.Length);

        for (int i = 0; i < num; i++)
        {
            GameObject newShip = Instantiate(shipPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
        }

        _ships = FindObjectsOfType<Ship>().ToList();
    }

    private IEnumerator SpawnPowerup()
    {
        if (powerupPrefabs.Length > 0 && powerupSpawnCenter)
        {
            int randInd = Random.Range(0, powerupPrefabs.Length);
            
            Vector3 spawnPosition = powerupSpawnCenter.position;
            spawnPosition.x += Random.Range(0, powerupSpawnRadius);
            spawnPosition.z += Random.Range(0, powerupSpawnRadius);
            
            Instantiate(powerupPrefabs[randInd], spawnPosition, Quaternion.identity);
        }

        float normalizedTime = 0f;
        while (normalizedTime <= 1f)
        {
            normalizedTime += Time.deltaTime / powerupsInterval;
            yield return null;
        }

        StartCoroutine(SpawnPowerup());
    }

    public void RemoveShip(Ship ship)
    {
        _ships.Remove(ship);
        foreach (Ship aliveShip in _ships)
        {
            aliveShip.CheckDeadShip(ship);
        }

        if (_ships.Count < 2) gameOver = true;
    }
}