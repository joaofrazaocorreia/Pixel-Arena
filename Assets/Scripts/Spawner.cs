using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private int   minPlayers = 2;
    [SerializeField] private float timeForFirstSpawn = 2;
    [SerializeField] private float wildSpawnInterval = 30;
    [SerializeField] private float playerSpawnInterval = 5;
    [SerializeField] private float incrementInterval = 2;
    [SerializeField] private int increments = 2;
    [SerializeField] private int wildSpawnCount = 5;
    [SerializeField] private NetworkVariable<int> BLUEplayerWeakSpawnCount = new(1);
    [SerializeField] private NetworkVariable<int> REDplayerWeakSpawnCount = new(1);
    [SerializeField] private NetworkVariable<int> BLUEplayerRegularSpawnCount = new(0);
    [SerializeField] private NetworkVariable<int> REDplayerRegularSpawnCount = new(0);
    [SerializeField] private NetworkVariable<int> BLUEplayerTankSpawnCount = new(0);
    [SerializeField] private NetworkVariable<int> REDplayerTankSpawnCount = new(0);
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Enemy[] REDPrefabs;
    [SerializeField] private Transform REDSpawn;
    [SerializeField] private Enemy[] BLUEPrefabs;
    [SerializeField] private Transform BLUESpawn;
    [SerializeField] private float spawnWeakCost = 1f;
    [SerializeField] private float spawnRegularCost = 3f;
    [SerializeField] private float spawnTankCost = 5f;              

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
        Vector3 posBlue = BLUESpawn.position;
        Vector3 posRed = REDSpawn.position;

        for (int i = 0; i < BLUEplayerWeakSpawnCount.Value; i++)
        {   
            float x = Random.Range(posBlue.x - 20, posBlue.x + 20);
            float y = Random.Range(posBlue.y - 20, posBlue.y + 20);

            var newEnemy = Instantiate(BLUEPrefabs[1], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < REDplayerWeakSpawnCount.Value; i++)
        {   
            float x = Random.Range(posRed.x - 20, posRed.x + 20);
            float y = Random.Range(posRed.y - 20, posRed.y + 20);

            var newEnemy = Instantiate(REDPrefabs[1], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < BLUEplayerRegularSpawnCount.Value; i++)
        {   
            float x = Random.Range(posBlue.x - 20, posBlue.x + 20);
            float y = Random.Range(posBlue.y - 20, posBlue.y + 20);

            var newEnemy = Instantiate(BLUEPrefabs[0], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < REDplayerRegularSpawnCount.Value; i++)
        {   
            float x = Random.Range(posRed.x - 20, posRed.x + 20);
            float y = Random.Range(posRed.y - 20, posRed.y + 20);

            var newEnemy = Instantiate(REDPrefabs[0], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < BLUEplayerTankSpawnCount.Value; i++)
        {   
            float x = Random.Range(posBlue.x - 20, posBlue.x + 20);
            float y = Random.Range(posBlue.y - 20, posBlue.y + 20);

            var newEnemy = Instantiate(BLUEPrefabs[2], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }

        for (int i = 0; i < REDplayerTankSpawnCount.Value; i++)
        {   
            float x = Random.Range(posRed.x - 20, posRed.x + 20);
            float y = Random.Range(posRed.y - 20, posRed.y + 20);

            var newEnemy = Instantiate(REDPrefabs[2], new Vector3(x, y, 0), Quaternion.identity);
            var networkObject = newEnemy.GetComponent<NetworkObject>();

            networkObject.Spawn();
        }
    }

    public void IncreasePlayerWeakSpawnCount(PlayerTower player)
    {
        Debug.Log("increasing " + player.faction);
        if(player.mana >= spawnWeakCost)
        {
            switch(player.faction)
            {
                case Faction.Red:
                    IncreaseRedWeakSpawnCountServerRpc();
                    Debug.Log("increased weak red");
                    break;
                case Faction.Blue:
                    IncreaseBlueWeakSpawnCountServerRpc();
                    Debug.Log("increased weak blue");
                    break;
                default:
                    break;
            }

            player.RemoveMana(spawnWeakCost);
        }
    }

    public void IncreasePlayerRegularSpawnCount(PlayerTower player)
    {
        Debug.Log("increasing " + player.faction);
        if(player.mana >= spawnRegularCost)
        {
            switch(player.faction)
            {
                case Faction.Red:
                    IncreaseRedRegularSpawnCountServerRpc();
                    Debug.Log("increased reg red");
                    break;
                case Faction.Blue:
                    IncreaseBlueRegularSpawnCountServerRpc();
                    Debug.Log("increased ref blue");
                    break;
                default:
                    break;
            }

            player.RemoveMana(spawnRegularCost);
        }
    }

    public void IncreasePlayerTankSpawnCount(PlayerTower player)
    {
        Debug.Log("increasing " + player.faction);
        if(player.mana >= spawnTankCost)
        {
            switch(player.faction)
            {
                case Faction.Red:
                    IncreaseRedTankSpawnCountServerRpc();
                    Debug.Log("increased red tank");
                    break;
                case Faction.Blue:
                    IncreaseBlueTankSpawnCountServerRpc();
                    Debug.Log("increased blue tank");
                    break;
                default:
                    break;
            }

            player.RemoveMana(spawnTankCost);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseBlueWeakSpawnCountServerRpc()
    {
        Debug.Log("Increased Blue");
        BLUEplayerWeakSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseRedWeakSpawnCountServerRpc()
    {
        Debug.Log("Increased Red");
        REDplayerWeakSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseBlueRegularSpawnCountServerRpc()
    {
        Debug.Log("Increased Blue");
        BLUEplayerRegularSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseRedRegularSpawnCountServerRpc()
    {
        Debug.Log("Increased Red");
        REDplayerRegularSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseBlueTankSpawnCountServerRpc()
    {
        Debug.Log("Increased Blue");
        BLUEplayerTankSpawnCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseRedTankSpawnCountServerRpc()
    {
        Debug.Log("Increased Red");
        REDplayerTankSpawnCount.Value++;
    }
}
