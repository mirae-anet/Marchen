using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RockHandler : NetworkBehaviour
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
    int damageAmount;
    [SerializeField]
    float checkRadius = 0.5f;
    [SerializeField]
    float damageRadius = 4;
    [SerializeField]
    int bulletSpeed;

    //other components
    NetworkObject networkObject;

    public virtual void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
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
        if(Object.HasStateAuthority)
        {
            Ratate();
            Move();

            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            CheckForImpactPoint();
        }
    }

    public void Ratate(){
        transform.Rotate(Vector3.right * 100 * Time.deltaTime);
    }

    protected virtual void Move()
    {
        transform.position += transform.forward * Runner.DeltaTime * bulletSpeed;
    }

    protected virtual void CheckForImpactPoint()
    {
        int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, checkRadius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

        if(hitCount > 0)
        {
            //Now we need to figure out of anything was within the blast radius
            hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, damageRadius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

            //Deal damage to anything within the hit radius
            for(int i = 0; i < hitCount; i++)
            {
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
    //When despawning the object we want to create a visual explosion
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(isExplosion)
            Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.transform.position, Quaternion.identity);
    }
}
