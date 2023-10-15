using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @brief 에너미 움직임에 관한 클래스.
/// @details 어그로가 끌리지 않은 상태, 어그로가 끌린 상태에서 이동을 담당.
public class NetworkEnemyController : NetworkBehaviour
{
    private bool isThinking = false;
    private int isMove = 0;
    private bool isChase = false;

    [Header("설정")]
    /// @brief 어그로가 끌리지 않은 상태에서 이동 거리.
    public float moveDis = 3f;
    /// @brief 에너미 움직임 속도.
    public float moveSpeed = 5f;

    private int moveDir = 0;

    //other component
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    private NavMeshAgent nav;
    private TargetHandler targetHandler;
    private EnemyAttackHandler enemyAttackHandler;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        nav = GetComponent<NavMeshAgent>();
        targetHandler = GetComponent<TargetHandler>();
        enemyAttackHandler = GetBehaviour<EnemyAttackHandler>();
    }
    void Start()
    {
        // if(!Object.HasStateAuthority)
        //     StartCoroutine("Think", (Random.Range(0.5f, 4f))); // 논어그로
    }

    /// @brief 어그로가 끌렸는지 확인 후 다음 행동을 선택.
    public override void FixedUpdateNetwork() 
    {
        if(!Object.HasStateAuthority)
            return;

        if (enemyHPHandler.GetIsDead())
        {
            nav.isStopped = true;
            StopAllCoroutines();
            return;
        }

        if (targetHandler.GetIsAggro())
        {
            StopCoroutine("ThinkCO");
            isThinking = false;
            targetHandler.TargetisAlive();
            EnemyChase();
            enemyAttackHandler.Aiming();
            // AttackCancel(); // EnemyHPHandler에서 호출하도록 수정
        }
        else
        {
            if(!isThinking)
            {
                StartCoroutine("ThinkCO", (Random.Range(0.5f, 4f)));
                isThinking = true;
            }
            EnemyWander();
        }
    }

    /// @brief 어그로 아닐 때 이동
    void EnemyWander() 
    {
        // transform.position += transform.forward* moveSpeed * isMove * Runner.DeltaTime;
        nav.Move(transform.forward * moveSpeed * isMove * Runner.DeltaTime);
    }
    /// @brief 어그로 아닐 때 주변을 배회하도록 이동 방향을 설정.
    /// @param worry 멈춰서 고민하는 시간.
    IEnumerator ThinkCO(float worry) 
    {
        yield return new WaitForSeconds(worry);     // 고민
        moveDir = Random.Range(0, 360);             // 랜덤 방향 이동
        transform.Rotate(0, moveDir, 0);
        isMove = 1;
        RPC_animatonSetBool("isWalk", true);

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤
        RPC_animatonSetBool("isWalk", false);

        yield return new WaitForSeconds(worry);     // 고민
        moveDir = -180;                             // 되돌아감
        transform.Rotate(0, moveDir, 0);
        isMove = 1;
        RPC_animatonSetBool("isWalk", true);

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤
        RPC_animatonSetBool("isWalk", false);

        isThinking = false;
    }

    /// @brief 타겟을 향해서 이동.
    void EnemyChase()
    {
        if (!nav.enabled)
            return;
        
        Transform target = targetHandler.GetTarget();
        
        if(target != null)
            nav.SetDestination(target.position);

        if(!isChase || enemyHPHandler.GetIsDamage())
        {
            nav.isStopped = true;
        }
        else
        {
            nav.isStopped = false;
        }
    }

    /*
    public void SetNavEnabled(bool bol)
    {
        // isChase = bol;
        nav.enabled = bol;
    }
    */

    /// @brief isChase을 변경. 현재 에너미가 타겟을 추격 중인지 표시.
    public void SetIsChase(bool bol)
    {
        isChase = bol;
    }

    /// @brief 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
        // anim.SetTrigger("doJump");
    }

    /// @brief 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
}
