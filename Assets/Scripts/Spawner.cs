using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private int   minPlayers = 2;
    [SerializeField] private float timeForFirstSpawn = 2;
    [SerializeField] private float wildSpawnInterval = 30;
    [SerializeField] private float playerSpawnInterval = 5;
    [SerializeField] private float incrementInterval = 2;
    [SerializeField] private int increments = 2;
    [SerializeField] private int wildSpawnCount = 5;
    [SerializeField] private NetworkVariable<int> BLUEplayerSpawnCount = new(1);
    [SerializeField] private NetworkVariable<int> REDplayerSpawnCount = new(1);
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Enemy[] REDPrefabs;
    [SerializeField] private Transform REDSpawn;
    [SerializeField] private Enemy[] BLUEPrefabs;
    [SerializeField] private Transform BLUESpawn;
    [SerializeField] private float spawnCost = 1f;              

    private float                wildSpawnTimer;
    private float                playerSpawnTimer;
    private float                incrementCounter;
    public int                   currentPlayers => NetworkManager.Singleton.ConnectedClients.Count;
    
    private PlayerTower localPlayer;

    private void Start()
    {
        wildSpawnTimer = timeForFirstSpawn;
        playerSpawnTimer = timeForFirstSpawn;
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

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            if (currentPlayers >= minPlayers)
            {
                wildSpawnTimer -= Time.deltaTime;
                if (wildSpawnTimer <= 0.0f)
                {
                    SpawnWildEnemies();
                    wildSpawnTimer = wildSpawnInterval;
                }

                playerSpawnTimer -= Time.deltaTime;
                if (playerSpawnTimer <= 0.0f)
                {
                    SpawnTeamEnemies();
                    playerSpawnTimer = playerSpawnInterval;
                }
            }

            else if (currentPlayers < minPlayers)
            {
                var enemies = FindObjectsOfType<Enemy>();
                foreach (var enemy in enemies)
                {
                    Destroy(enemy.gameObject);
                }
                wildSpawnTimer = timeForFirstSpawn;
            }
        }
    }

    void SpawnWildEnemies()
    {
        Debug.Log(wildSpawnCount + "; " + incrementCounter + "; " + incrementCounter % incrementInterval);
        for (int i = 0; i < wildSpawnCount; i++)
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
            wildSpawnCount += increments;
        }
    }

    void SpawnTeamEnemies()
    {
        Debug.Log(BLUEplayerSpawnCount.Value + "; " + REDplayerSpawnCount.Value);

        Enemy chosenEnemy;
        Vector3 posBlue = BLUESpawn.position;
        Vector3 posRed = REDSpawn.position;

        for (int i = 0; i < BLUEplayerSpawnCount.Value; i++)
        {   
            chosenEnemy = BLUEPrefabs[Random.Range(0, BLUEPrefabs.Length)];

            float x = Random.Range(posBlue.x - 20, posBlue.x + 20);
            float y = Random.Range(posBlue.y - 20, posBlue.y + 20);

            var newEnemy = Instantiate(chosenEnemy, new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < REDplayerSpawnCount.Value; i++)
        {   
            chosenEnemy = REDPrefabs[Random.Range(0, REDPrefabs.Length)];

            float x = Random.Range(posRed.x - 20, posRed.x + 20);
            float y = Random.Range(posRed.y - 20, posRed.y + 20);

            var newEnemy = Instantiate(chosenEnemy, new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }
    }

    public void IncreasePlayerSpawnCount(PlayerTower player)
    {
        Debug.Log("increasing " + player.faction);
        if(player.mana >= spawnCost)
        {
            switch(player.faction)
            {
                case Faction.Red:
                    IncreaseRedSpawnCountServerRpc();
                    Debug.Log("increased red");
                    break;
                case Faction.Blue:
                    IncreaseBlueSpawnCountServerRpc();
                    Debug.Log("increased blue");
                    break;
                default:
                    break;
            }

            player.RemoveMana(spawnCost);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseBlueSpawnCountServerRpc()
    {
        Debug.Log("Increased Blue");
        BLUEplayerSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseRedSpawnCountServerRpc()
    {
        Debug.Log("Increased Red");
        REDplayerSpawnCount.Value++;
    }
}
