using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int   minPlayers = 2;
    [SerializeField] private float timeForFirstSpawn = 2;
    [SerializeField] private float spawnInterval = 10;
    [SerializeField] private int   spawnCount = 5;
    [SerializeField] private Enemy[] enemyPrefabs;

    private float          spawnTimer;
    private NetworkManager networkManager;
    private int            currentPlayers => networkManager.ConnectedClients.Count;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        spawnTimer = timeForFirstSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        if (networkManager.IsServer || networkManager.IsHost)
        {
            if (currentPlayers >= minPlayers)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0.0f)
                {
                    Spawn();
                    spawnTimer = spawnInterval;
                }
            }
            else if (currentPlayers == 0)
            {
                var enemies = FindObjectsOfType<Enemy>();
                foreach (var enemy in enemies)
                {
                    Destroy(enemy.gameObject);
                }
                spawnTimer = timeForFirstSpawn;
            }
        }
    }

    void Spawn()
    {
        // Get all players
        var PlayerTowers = FindObjectsOfType<PlayerTower>();
        if (PlayerTowers.Length == 0) return;

        float xMin = PlayerTowers[0].transform.position.x;
        float yMin = PlayerTowers[0].transform.position.y;
        float xMax = xMin;
        float yMax = yMin;

        foreach (var PlayerTower in PlayerTowers)
        {
            xMin = Mathf.Min(xMin, PlayerTower.transform.position.x);
            xMax = Mathf.Max(xMax, PlayerTower.transform.position.x);
            yMin = Mathf.Min(yMin, PlayerTower.transform.position.y);
            yMax = Mathf.Max(yMax, PlayerTower.transform.position.y);
        }

        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(xMin - 20, xMax + 20);
            float y = Random.Range(yMin - 20, yMax + 20);

            Enemy chosenEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            var newEnemy = Instantiate(chosenEnemy, new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }
    }
}
