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
    
    //prefab
    public BulletHandler bulletPrefab;

    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
    }

    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        if(Physics.SphereCast(transform.position, targetRadius, transform.forward, out var hitInfo, targetRange, LayerMask.GetMask("Player")) )
        {
            if(!isAttack && !enemyHPHandler.GetIsDamage())
                StartCoroutine("AttackCO");
        }
    }

    IEnumerator AttackCO()
    {
        networkEnemyController.SetIsChase(false);
        isAttack = true;

        yield return new WaitForSeconds(0.3f);

        RPC_animatonSetBool("isAttack", true);
        yield return new WaitForSeconds(0.5f);

        Runner.Spawn(bulletPrefab, anchorPoint.position, Quaternion.LookRotation(transform.forward), Object.StateAuthority, (runner, spawnedBullet) =>
        {
            spawnedBullet.GetComponent<BulletHandler>().Fire(Object.StateAuthority, Object, transform.name);
        });

        yield return new WaitForSeconds(2f);

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
