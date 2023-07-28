using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 원거리 공격 에너미의 공격 관련 클래스.
public class RangeAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;
    /// @brief 에너미의 공격이 취소될 수 있는지 설정. default는 true.
    public bool attackCancel = true;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (반지름)
    public float targetRadius =  0.5f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (최대 거리)
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
    public AudioSource attackSound;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        targetHandler = GetComponent<TargetHandler>();
    }

    /// @brief 타겟을 향해서 공격을 준비.
    /// @details SphereCastAll로 타겟을 탐색. hit 시 AttackCO를 호출.
    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        if(isAttack || enemyHPHandler.GetIsDamage())
            return;

        RaycastHit[] rayhits = Physics.SphereCastAll(detectionPos.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if(rayhits.Length > 0)
        {
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

    /// @brief 공격 실행.
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

    /// @brief 공격 취소.
    public override void AttackCancel()
    {
        if (!attackCancel)
            return;

        StopCoroutine("AttackCO");

        networkEnemyController.SetIsChase(true);
        isAttack = false;
        RPC_animatonSetBool("isAttack", false);
    }

    /// @brief 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    /// @brief 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
    /// @brief 효과음 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AudioPlay(string audioType)
    {
        attackSound.Play();
    }
}
