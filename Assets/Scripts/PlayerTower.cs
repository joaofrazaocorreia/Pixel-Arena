using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerTower : Character
{
    [SerializeField] private Transform              arm;
    [SerializeField] private NetworkVariable<float> cooldown = new(0.25f);
    [SerializeField] private NetworkVariable<float> damage = new(10.0f);
    [SerializeField] private Projectile             shotPrefab;
    [SerializeField] private Projectile             shotPrefabNetwork;
    [SerializeField] private Transform              shootPoint;
    [SerializeField] private float                  manaRegenRate = 1.0f;
    [SerializeField] private float                  range = 100.0f;
    [SerializeField] private float                  attackUPGcost = 3f;   
    [SerializeField] private float                  speedUPGcost = 3f;  

    float   cooldownTimer;
    NetworkVariable<int>      _mana = new(3);
    NetworkVariable<float>    _phantomMana = new(3);
    NetworkVariable<float>    _maxMana = new(10);
    public float mana => _mana.Value;
    public float phantomMana => _phantomMana.Value;
    public float maxMana => _maxMana.Value;


    protected override void Start()
    {
        base.Start();

        cooldownTimer = cooldown.Value;
    }

    void Update()
    {
        if (isDead)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
            return;
        }

        var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Enemy closestEnemy = null;
        float minDist = range;

        // Find closest
        foreach (var enemy in enemies)
        {
            if (enemy.isDead || enemy.faction == faction) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestEnemy = enemy;
            }
        }

        Vector3 targetVector = Vector3.down;
        if (closestEnemy != null)
        {
            targetVector = (closestEnemy.transform.position + (Vector3.up * 8) - arm.transform.position).normalized;
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetVector);

        arm.transform.rotation = Quaternion.RotateTowards(arm.transform.rotation, targetRotation, Time.deltaTime * 360.0f);

        if ((closestEnemy != null) && (shotPrefab != null) && networkObject.IsLocalPlayer)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0.0f)
            {
                Shoot(shootPoint.position, shootPoint.rotation);

                cooldownTimer = cooldown.Value;
            }
        }

        UpdateAnimation();

        if(FindObjectsOfType<PlayerTower>().Count() >= 2)
            AddMana(manaRegenRate / _maxMana.Value * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
    }

    protected void Shoot(Vector3 pos, Quaternion rotation)
    {
        var spawnedObject = Instantiate(shotPrefab, pos, rotation);
        spawnedObject.projectileId = projectileId;
        spawnedObject.origin = pos;
        spawnedObject.direction = rotation * Vector3.up;
        spawnedObject.shotTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        spawnedObject.damage = damage.Value;
        spawnedObject.playerId = networkObject.OwnerClientId;

        ShootServerRpc(pos, rotation, spawnedObject.shotTime, networkObject.OwnerClientId, projectileId);

        projectileId++;
    }

    [ServerRpc]
    protected void ShootServerRpc(Vector3 pos, Quaternion rotation, float shotTime, ulong playerId, int projectileId)
    {
        var spawnedObject = Instantiate(shotPrefabNetwork, pos, rotation);
        spawnedObject.projectileId = projectileId;
        spawnedObject.playerId = playerId;
        spawnedObject.origin = pos;
        spawnedObject.direction = rotation * Vector3.up;
        spawnedObject.shotTime = shotTime;
        spawnedObject.damage = damage.Value;
        var netObj = spawnedObject.GetComponent<NetworkObject>();

        netObj.Spawn();

        spawnedObject.SyncClients();
    }


    public void AddMana(float amount)
    {
        _phantomMana.Value = Mathf.Clamp(_phantomMana.Value + amount, 0f, maxMana);
        _mana.Value = (int) Mathf.Floor(_phantomMana.Value);
    }

    public void RemoveMana(float amount)
    {
        _phantomMana.Value = Mathf.Clamp(_phantomMana.Value - amount, 0f, maxMana);
        _mana.Value = (int) Mathf.Floor(_phantomMana.Value);
    }

    public void Upgrade(string upgradeName)
    {
        UpgradeClientServerRpc(upgradeName);
    }

    [ServerRpc]
    public void UpgradeClientServerRpc(string upgradeName)
    {
        switch (upgradeName)
        {
            case "speed":
                if(mana >= speedUPGcost)
                {
                    cooldown.Value *= 0.75f;
                    RemoveMana((int) speedUPGcost);
                }
                break;
            case "attack":
                if(mana >= attackUPGcost)
                {
                    damage.Value *= 1.5f;
                    RemoveMana((int) attackUPGcost);
                }
                break;
            default:
                break;
        }
    }
}
