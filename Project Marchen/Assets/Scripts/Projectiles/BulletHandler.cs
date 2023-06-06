using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BulletHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    GameObject explosionParticleSystemPrefab;

    [Header("Collsion")]
    [SerializeField]
    private Transform checkForImpactPoint;
    [SerializeField]
    private LayerMask collisionLayers;
    public bool isExplosion;

    //Thrown by info
    PlayerRef firedByPlayerRef;
    string firedByName;
    NetworkObject firedByNetworkObject;

    //Hit info
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    
    //Timing
    TickTimer maxLiveDurationTickTimer = TickTimer.None;
    
    //Rocket info
    [Header("Bullet info")]
    [SerializeField]
    byte damageAmount;
    [SerializeField]
    float radius = 4;
    [SerializeField]
    int bulletSpeed;

    //other components
    NetworkObject networkObject;

    public void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;
        
        networkObject = GetComponent<NetworkObject>();

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * Runner.DeltaTime * bulletSpeed;

        if(Object.HasStateAuthority)
        {
            //Check if the rocket has reached the end of its life
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            //Check if the rocket has hit anything
            int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 0.5f, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);
            bool isValidHit = false;

            if(hitCount > 0)
                isValidHit = true;
            
            //check what we've hit
            for(int i=0; i < hitCount; i++)
            {
                if(hits[i].Hitbox != null)
                    if(hits[i].Hitbox.Root.GetBehaviour<NetworkObject>() == firedByNetworkObject)
                        isValidHit = false;
            }

            //We hit something valid
            if(isValidHit)
            {
                //Now we need to figure out of anything was within the blast radius
                hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, radius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

                //Deal damage to anything within the hit radius
                for(int i = 0; i < hitCount; i++)
                {
                    //BUG!!!
                    // HPHandler hpHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();
                    // if(hpHandler != null && firedByNetworkObject != null)

                    if(hits[i].Hitbox.Root.TryGetComponent<HPHandler>(out HPHandler hpHandler))
                    {
                        if(firedByNetworkObject != null)
                            hpHandler.OnTakeDamage(firedByName, damageAmount, transform.position);
                    }
                    if(hits[i].Hitbox.Root.transform.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
                    {
                        if(firedByNetworkObject != null)
                            enemyHPHandler.OnTakeDamage(firedByName, firedByNetworkObject, damageAmount, transform.position);
                    }
                }

                Runner.Despawn(networkObject);
            }

        }
    }

    //When despawning the object we want to create a visual explosion
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(isExplosion)
            Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.transform.position, Quaternion.identity);
    }
}
