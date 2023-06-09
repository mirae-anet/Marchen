using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GrenadeHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject explosionParticleSystemPrefab;
    [Header("Collsion")]
    public LayerMask collisionLayers;

    [Header("Grenade damage")]
    [SerializeField]
    int damageAmount;

    //Thrown by info
    PlayerRef thrownByPlayerRef;
    string thrownByPlayerName;
    NetworkObject thrownByNetworkObject;

    //Hit info
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    
    //Timing
    TickTimer explodeTickTimer = TickTimer.None;

    //other components
    NetworkObject networkObject;
    NetworkRigidbody networkRigidbody;

    public void Throw(Vector3 throwForce, PlayerRef thrownByPlayerRef, NetworkObject thrownByNetworkObject,string thrownByPlayerName)
    {
        networkObject = GetComponent<NetworkObject>();
        networkRigidbody = GetComponent<NetworkRigidbody>();

        networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse);
        this.thrownByPlayerRef = thrownByPlayerRef;
        this.thrownByPlayerName = thrownByPlayerName;
        this.thrownByNetworkObject = thrownByNetworkObject;

        explodeTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
    }

    //Network update
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            if(explodeTickTimer.Expired(Runner))
            {
                int hitCount = Runner.LagCompensation.OverlapSphere(transform.position, 4, thrownByPlayerRef, hits, collisionLayers);

                for(int i =0; i < hitCount; i++)
                {
                    EnemyHPHandler enemyHpHandler = hits[i].Hitbox.Root.transform.GetComponent<EnemyHPHandler>();

                    if(enemyHpHandler != null)
                        enemyHpHandler.OnTakeDamage(thrownByPlayerName, thrownByNetworkObject,damageAmount, transform.position);
                }

                Runner.Despawn(networkObject);

                //Stop the explode timer from being triggered again
                explodeTickTimer = TickTimer.None;
            }
        }
    }

    //When despawning the object we want to create a visual explosion
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

        Instantiate(explosionParticleSystemPrefab, grenadeMesh.transform.position, Quaternion.identity);
    }
}
