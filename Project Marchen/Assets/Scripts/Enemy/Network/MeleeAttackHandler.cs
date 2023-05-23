using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MeleeAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;
    public bool attackCancel = true;
    // public float targetRadius =  1.5f;
    public float targetRange = 3f;
    public Vector3 boxSize = new Vector3(2f, 2f, 2f);
    public byte damageAmount = 10;
    public Transform anchorPoint;
    
    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    TargetHandler targetHandler;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        targetHandler = GetComponent<TargetHandler>();
    }

    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        // if(Physics.BoxCast(transform.position, boxSize/2, transform.forward, Quaternion.identity, targetRange, LayerMask.GetMask("Player")) )
        // {
        //     if(!isAttack && !enemyHPHandler.GetIsDamage())
        //         StartCoroutine("AttackCO");
        // }
        if(isAttack || enemyHPHandler.GetIsDamage())
            return;

        RaycastHit[] rayhits = Physics.BoxCastAll(transform.position, boxSize/2, transform.forward, Quaternion.LookRotation(transform.forward), targetRange, LayerMask.GetMask("Player"));

        if(rayhits.Length > 0)
        {
            // foreach(RaycastHit rayhit in rayhits) // target 확인
            for(int i = 0; rayhits.Length > i; i++)
            {
                
                Transform player = rayhits[i].collider.transform.root.transform;
                if(targetHandler.GetTarget() == player)
                {
                    StartCoroutine("AttackCO");
                    break;
                }
            }
        }
    }

    IEnumerator AttackCO()
    {
        networkEnemyController.SetIsChase(false);
        isAttack = true;

        yield return new WaitForSeconds(0.3f);

        RPC_animatonSetBool("isAttack", true);
        yield return new WaitForSeconds(0.5f);

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        float endTime = Time.time + 1f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.identity, Object.StateAuthority, hits, LayerMask.GetMask("PlayerHitBox"));
            if(hitCount > 0)
            {
                for(int i = 0; i < hitCount; i++)
                {
                    HPHandler hpHandler = hits[i].Hitbox.Root.GetComponent<HPHandler>();

                    if(hpHandler != null)
                        hpHandler.OnTakeDamage(transform.name, damageAmount, transform.position);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
        RPC_animatonSetBool("isAttack", false);
    }

    public override void AttackCancel()
    {
        if (!attackCancel)
            return;

        StopCoroutine("AttackCO");

        networkEnemyController.SetIsChase(true);
        isAttack = false;
        RPC_animatonSetBool("isAttack", false);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
}
