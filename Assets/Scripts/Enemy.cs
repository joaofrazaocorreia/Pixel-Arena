using UnityEngine;

public class Enemy : Character
{
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] float damage = 200.0f;
    [SerializeField] float range = 15.0f;

    float cooldownTimer;

    protected override void Start()
    {
        base.Start();

        healthSystem.onDeath += HealthSystem_onDeath;

        cooldownTimer = cooldown;
    }

    private void HealthSystem_onDeath()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if(networkObject.NetworkManager.IsServer)
            Think();

        UpdateAnimation();
    }

    void Think()
    {
        var PlayerTowers = FindObjectsByType<PlayerTower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var Enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        float minDist = float.MaxValue;
        Character closestTarget = null;
        

        // Find closest
        foreach (var PlayerTower in PlayerTowers)
        {
            if (PlayerTower.isDead || PlayerTower.faction == faction) continue;

            float dist = Vector3.Distance(transform.position, PlayerTower.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestTarget = PlayerTower;
            }
        }
        
        foreach (var Enemy in Enemies)
        {
            if (Enemy.isDead || Enemy.faction == faction) continue;

            float dist = Vector3.Distance(transform.position, Enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestTarget = Enemy;
            }
        }

        if (closestTarget == null)
        {
            return;
        }

        if (minDist > range)
        {
            Vector3 dir = (closestTarget.transform.position - transform.position).normalized;

            Vector3 moveVector = dir * speed;

            transform.Translate(moveVector * Time.deltaTime, Space.World);
        }

        else
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0.0f)
            {
                cooldownTimer = cooldown;

                closestTarget.DealDamage(damage);
            }
        }
    }
}
