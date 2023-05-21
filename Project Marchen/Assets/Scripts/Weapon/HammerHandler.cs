using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class HammerHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    private Transform anchorPoint;
    private Vector3 boxSize = new Vector3(2f, 2f, 2f);

    [SerializeField]
    private TrailRenderer trailEffect;

    [Header("설정")]
    public Type type = Type.Melee;
    [Range(1f, 100f)]
    public byte damageAmount = 25;
    [Range(0f, 5f)]
    public float delay = 0.6f;

    //other compomponet
    Animator anim;
    NetworkPlayerController networkPlayerController;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
        networkPlayerController = GetComponentInParent<NetworkPlayerController>();
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        networkObject = GetComponentInParent<NetworkObject>();
    }

    public override void Attack(Vector3 aimDir)
    {
        anim.SetTrigger("doSwing");
        StartCoroutine("Swing");
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.2f);
        trailEffect.enabled = true;

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        float endTime = Time.time + 3f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(transform.forward), Object.InputAuthority, hits, LayerMask.GetMask("EnemyHitBox"));
            if(hitCount > 0)
            {
                for(int i = 0; i < hitCount; i++)
                {
                    EnemyHPHandler enemyHPHandler = hits[i].Hitbox.Root.GetComponent<EnemyHPHandler>();

                    if(enemyHPHandler != null)
                        enemyHPHandler.OnTakeDamage(networkPlayer.nickName.ToString(), networkObject, damageAmount, transform.position);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        trailEffect.enabled = false;

        networkPlayerController.SetIsAttack(false);
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
}
