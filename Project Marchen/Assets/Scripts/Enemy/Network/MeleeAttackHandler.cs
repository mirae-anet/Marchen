using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 근거리 공격 에너미의 공격 관련 클래스.
public class MeleeAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;

    /// @brief 에너미의 공격이 취소될 수 있는지 설정. default는 true.
    public bool attackCancel = true;
    // public float targetRadius =  1.5f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (최대 거리)
    public float targetRange = 3f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (가로 세로 높이)
    public Vector3 boxSize = new Vector3(2f, 2f, 2f);
    /// @brief 입히는 피해량
    public int damageAmount = 10;
    public Transform anchorPoint;
    
    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    TargetHandler targetHandler;
    public AudioSource attackSound;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        targetHandler = GetComponent<TargetHandler>();
    }

    /// @brief 타겟을 향해서 공격을 준비.
    /// @details BoxCastAll로 타겟을 탐색. hit 시 AttackCO를 호출.
    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        if(enemyHPHandler == null)
            return;

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

    /// @brief 공격 실행.
    IEnumerator AttackCO()
    {
        networkEnemyController.SetIsChase(false);
        isAttack = true;

        yield return new WaitForSeconds(0.3f);

        RPC_animatonSetBool("isAttack", true);
        RPC_AudioPlay("attack");
        yield return new WaitForSeconds(0.5f);

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        float endTime = Time.time + 1f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(transform.forward), Object.StateAuthority, hits, LayerMask.GetMask("PlayerHitBox"));
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

        yield return new WaitForSeconds(0.3f);

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
