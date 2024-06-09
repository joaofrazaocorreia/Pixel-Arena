using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int   minPlayers = 2;
    [SerializeField] private float timeForFirstSpawn = 2;
    [SerializeField] private float spawnInterval = 30;
    [SerializeField] private float incrementInterval = 2;
    [SerializeField] private int increments = 2;
    [SerializeField] private int   spawnCount = 5;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Enemy[] REDPrefabs;
    [SerializeField] private Transform REDSpawn;
    [SerializeField] private Enemy[] BLUEPrefabs;
    [SerializeField] private Transform BLUESpawn;
    [SerializeField] private float spawnCost = 1f;              

    private float          spawnTimer;
    private float          incrementCounter;
    private NetworkManager networkManager;
    public int            currentPlayers => networkManager.ConnectedClients.Count;
    
    PlayerTower localPlayer;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        spawnTimer = timeForFirstSpawn;
        incrementCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            var players = FindObjectsOfType<PlayerTower>();
            
            foreach(PlayerTower p in players)
            {
                if (p.IsLocalPlayer)
                {
                    localPlayer = p;
                    break;
                }
            }
        }

        if (networkManager.IsServer || networkManager.IsHost)
        {
            if (currentPlayers >= minPlayers)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0.0f)
                {
                    SpawnWild();
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

    void SpawnWild()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(-50, 50);
            float y = Random.Range(100, 250);

            Enemy chosenEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            var newEnemy = Instantiate(chosenEnemy, new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        if(++incrementCounter % incrementInterval == 0)
        {
            spawnCount += increments;
        }
    }

    private void SpawnRed(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Enemy chosenEnemy = REDPrefabs[Random.Range(0, REDPrefabs.Length)];

            var newEnemy = Instantiate(chosenEnemy, REDSpawn.position, Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }
    }

    private void SpawnBlue(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Enemy chosenEnemy = BLUEPrefabs[Random.Range(0, BLUEPrefabs.Length)];

            var newEnemy = Instantiate(chosenEnemy, BLUESpawn.position, Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }
    }

    public void SpawnTeamEnemy()
    {
        if (localPlayer.mana >= spawnCost)
        {
            if (localPlayer.faction == Faction.Blue) SpawnBlue(1);
            if (localPlayer.faction == Faction.Red) SpawnRed(1);

            localPlayer.RemoveMana(spawnCost);
        }
    }
}
