using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StraightBulletHandler : NetworkBehaviour
{
    private bool isWait = false;
    public Transform anchorPoint;
    public LayerMask collisionLayers;
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
    public Vector3 boxSize;

    //other components
    NetworkObject networkObject;

    public void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;
        
        networkObject = GetComponent<NetworkObject>();

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        StartCoroutine("WaitCO");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
    }
    IEnumerator WaitCO()
    {
        yield return new WaitForSeconds(0.5f);
        isWait = true;
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            if(!isWait)
                return;
            
            //Check if the rocket has reached the end of its life
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(transform.forward), firedByPlayerRef, hits, collisionLayers, HitOptions.None);
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
        }
    }
}
