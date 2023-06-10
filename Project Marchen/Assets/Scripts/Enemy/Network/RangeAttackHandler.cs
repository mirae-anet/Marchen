using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RangeAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;
    public bool attackCancel = true;
    public float targetRadius =  0.5f;
    public float targetRange = 25f;
    public Transform anchorPoint;
    public Transform detectionPos;
    private Vector3 aimVec;
    
    //prefab
    public BulletHandler bulletPrefab;

    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    private TargetHandler targetHandler;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        targetHandler = GetComponent<TargetHandler>();
    }

    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        // if(Physics.SphereCast(transform.position, targetRadius, transform.forward, out var hitInfo, targetRange, LayerMask.GetMask("Player")) )
        // {
        //     if(!isAttack && !enemyHPHandler.GetIsDamage())
        //         StartCoroutine("AttackCO");
        // }
        if(isAttack || enemyHPHandler.GetIsDamage())
            return;

        RaycastHit[] rayhits = Physics.SphereCastAll(detectionPos.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if(rayhits.Length > 0)
        {
            // foreach(RaycastHit rayhit in rayhits) // target 확인
            for(int i = 0; rayhits.Length > i; i++)
            {
                
                Transform player = rayhits[i].collider.transform.root.transform;
                if(targetHandler.GetTarget() == player)
                {
                    aimVec = player.transform.position - transform.position;
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

        yield return new WaitForSeconds(0.1f);

        RPC_animatonSetBool("isAttack", true);
        yield return new WaitForSeconds(0.5f);

        Runner.Spawn(bulletPrefab, anchorPoint.position, Quaternion.LookRotation(aimVec.normalized), Object.StateAuthority, (runner, spawnedBullet) =>
        {
            spawnedBullet.GetComponent<BulletHandler>().Fire(Object.StateAuthority, Object, transform.name);
        });

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
