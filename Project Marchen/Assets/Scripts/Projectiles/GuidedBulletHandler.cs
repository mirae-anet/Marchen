using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class GuidedBulletHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public Transform checkForImpactPoint;
    public LayerMask collisionLayers;

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
    public int damageAmount;
    public float radius = 4;

    private Transform target;

    //other components
    NavMeshAgent nav;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    public void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 6);
    }

    public override void FixedUpdateNetwork()
    {
        Move();

        if(Object.HasStateAuthority)
        {
            //Check if the rocket has reached the end of its life
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
                return;
            }

            int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 1.25f, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);

            if(hitCount > 0)
            {
                //Now we need to figure out of anything was within the blast radius
                hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, radius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

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

                Runner.Despawn(Object);
            }
        }
    }

    private void Move()
    {
        if (target == null)
            return;

        nav.SetDestination(target.position);
    }

    public void setTarget(Transform vec)
    {
        target = vec;
    }
}
