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

    public Vector3 mapCenter = Vector3.zero;
    public Vector3 mapSize = new Vector3(1000, 0, 1000);

    public int ShipsLeft => _ships.Count;

    [Header("Ships Spawning")] [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField] private GameObject[] shipPrefabs;

    [Header("PowerUps Spawning")] [SerializeField]
    private GameObject[] powerupPrefabs;

    [SerializeField] private Transform powerupSpawnCenter;
    [SerializeField] [Range(1, 15)] private int maxPowerUpsNum = 5;
    [SerializeField] [Range(1, 30)] private float powerupsInterval = 10;
    [SerializeField] [Range(1, 150)] private float powerupSpawnRadius = 20;

    private HashSet<PowerUp> _powerupsSpawned = new HashSet<PowerUp>();

    private List<Ship> _ships = new List<Ship>();

    private void Awake()
    {
        current = this;

        SpawnShips();
        StartCoroutine(SpawnPowerup());
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
            _powerupsSpawned = new HashSet<PowerUp>(_powerupsSpawned.Where(powerUp => powerUp != null));

            if (_powerupsSpawned.Count < maxPowerUpsNum)
            {
                foreach (Ship ship in _ships) ship.UpdateVisiblePowerUps(_powerupsSpawned);
                
                int randInd = new System.Random().Next(0, powerupPrefabs.Length);

                Vector2 spawnPos = Random.insideUnitCircle * powerupSpawnRadius;

                GameObject newPowerUp = Instantiate(
                    powerupPrefabs[randInd],
                    new Vector3(
                        spawnPos.x,
                        powerupSpawnCenter.transform.position.y,
                        spawnPos.y
                    ),
                    Quaternion.identity
                );
                _powerupsSpawned.Add(newPowerUp.GetComponent<PowerUp>());
            }
        }

        yield return new WaitForSeconds(powerupsInterval);
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