using Unity.Netcode;
using UnityEngine;

public class Wyzard : Character
{
    [SerializeField] private Transform              arm;
    [SerializeField] private float                  cooldown = 0.25f;
    [SerializeField] private float                  damage = 10.0f;
    [SerializeField] private Projectile             shotPrefab;
    [SerializeField] private Projectile             shotPrefabNetwork;
    [SerializeField] private Transform              shootPoint;
    [SerializeField] private ParticleSystem         manaCountUpPS;

    float   cooldownTimer;
    NetworkVariable<int>    _mana = new(3);
    NetworkVariable<int>    _maxMana = new(10);
    public int mana => _mana.Value;
    public int maxMana => _maxMana.Value;


    protected override void Start()
    {
        base.Start();

        cooldownTimer = cooldown;
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

        if (networkObject.IsLocalPlayer)
        {
            Vector3 moveDir = Vector3.zero;
            moveDir.x = speed * Input.GetAxis("Horizontal");
            moveDir.y = speed * Input.GetAxis("Vertical");

            moveDir *= Time.deltaTime;

            transform.Translate(moveDir, Space.World);
        }

        var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Enemy closestEnemy = null;
        float minDist = float.MaxValue;

        // Find closest
        foreach (var enemy in enemies)
        {
            if (enemy.isDead) continue;

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

                cooldownTimer = cooldown;
            }
        }

        UpdateAnimation();
    }

    protected void Shoot(Vector3 pos, Quaternion rotation)
    {
        var spawnedObject = Instantiate(shotPrefab, pos, rotation);
        spawnedObject.projectileId = projectileId;
        spawnedObject.origin = pos;
        spawnedObject.direction = rotation * Vector3.up;
        spawnedObject.shotTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        spawnedObject.damage = damage;
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
        spawnedObject.damage = damage;
        var netObj = spawnedObject.GetComponent<NetworkObject>();

        netObj.Spawn();

        spawnedObject.SyncClients();
    }


    public void AddMana(int ammount)
    {
        _mana.Value += ammount;
    }
}
