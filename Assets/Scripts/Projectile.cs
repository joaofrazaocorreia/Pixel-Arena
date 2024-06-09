using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public Faction  faction;
    public float    speed = 200.0f;
    public float    damage = 100.0f;
    public float    duration = 0.5f;

    public float shotTime = 0;
    public Vector3 origin;
    public Vector3 direction;
    public int projectileId;
    public ulong playerId;
    
    private Vector3     prevPos;

    private void Start()
    {
        prevPos = origin;
    }

    void Update()
    {
        ComputePosition(); 

        if (NetworkManager.IsServer)
        {
            // Improved collision detection, works better on server side
            var hits = Physics2D.LinecastAll(prevPos, transform.position);
            foreach (var hit in hits)
            {
                var healthSystem = hit.collider.GetComponent<HealthSystem>();
                if ((healthSystem != null) && (!healthSystem.isDead))
                {
                    // Check factions
                    if (healthSystem.faction != faction)
                    {
                        // Run damage
                        healthSystem.DealDamage(damage);
                        Destroy(gameObject);
                    }
                }
            }

            duration -= Time.deltaTime;

            if (duration <= 0.0f)
            {
                Destroy(gameObject);
            }

            else
            {
                prevPos = transform.position;
            }
        }
    }


    void ComputePosition()
    {
        if(shotTime > 0)
        {
            float elapsedTime = NetworkManager.Singleton.ServerTime.TimeAsFloat - shotTime;
            transform.position = origin + direction * elapsedTime * speed;
        }
        else
        {
            float elapsedTime = NetworkManager.Singleton.ServerTime.TimeAsFloat - shotTime;
            transform.position = origin + direction * elapsedTime * speed;
        }
    }

    private void DestroyLocalProjectile(int projectileId, ulong playerId)
    {
        Projectile[] projectiles = FindObjectsOfType<Projectile>();
        foreach (var proj in projectiles)
        {
            // Use the new value of ProjectileId to find the predicted projectile
            if ((proj.playerId == playerId) && (proj.projectileId == projectileId) && (proj != this))
            {
                Destroy(proj.gameObject);
                break;
            }
        }
    }

    public void SyncClients()
    {
        SyncClientsClientRpc(origin, direction, shotTime, projectileId, playerId);
    }

    [ClientRpc]
    void SyncClientsClientRpc(Vector3 origin, Vector3 dir, float shotTime, int projectileId, ulong playerId)
    {
        this.origin = prevPos = origin;
        this.direction = dir;
        this.shotTime = shotTime;
        this.projectileId = projectileId;
        this.playerId = playerId;

        DestroyLocalProjectile(projectileId, playerId);
    }
}
