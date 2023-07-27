using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @breif 에너미의 타겟을 관리하는 클래스
public class TargetHandler : NetworkBehaviour
{
    /// @breif 지정된 타겟
    private Transform target;
    /// @breif 지정된 타겟의 존재 여부
    private bool isAggro = false;

    [SerializeField]
    private GameObject agrroPulling;

    //other component
    private Animator anim;
    private NetworkEnemyController networkEnemyController;
    // private NavMeshAgent nav;

    void Awake()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        // nav = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// @breif 타겟을 설정.
    /// @param target 새로 설정할 타겟.
    public void SetTarget(Transform target) // 타겟 (재)설정
    {
        Debug.Log(gameObject.name + " target set");
        this.target = target;
        isAggro = true;
        agrroPulling.SetActive(false);
        StartCoroutine(ChaseStartCO());
    }

    /// @breif 타겟을 추적하기 시작. 
    IEnumerator ChaseStartCO()
    {
        yield return new WaitForSeconds(0.1f);
        networkEnemyController.SetIsChase(true); //single code에선 SetTarget에서 이미 호출함 그래서 효과없음.
        RPC_animatonSetBool("isWalk", true);
    }
    
    /// @breif 타겟의 사라지거나, 죽은거 확인
    public void TargetisAlive() 
    {
        if (target == null) // 타겟이 없으면
        {
            TargetOff();
            return;
        }
        else if (target.gameObject.GetComponent<HPHandler>().GetIsDead()) // 타겟이 죽으면
        {
            TargetOff();
            return;
        }
        else // 타겟이 있고 살아 있다면
            return;
    }

    /// @breif 타겟 해제
    void TargetOff() 
    {
        RPC_animatonSetBool("isWalk", false);
        RPC_animatonSetBool("isAttack", false);

        networkEnemyController.SetIsChase(false);
        agrroPulling.SetActive(true);
        isAggro = false;
        Debug.Log("TargetOff");
    }

    /// @return Transform target
    public Transform GetTarget()
    {
        return target;
    }
    /// @return bool isAggro
    public bool GetIsAggro()
    {
        return isAggro;
    }

    /// @breif 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    /// @breif 애니메이션 동기화.
    /// @details 서버가 모든 컴퓨터에서 실행하도록 지시. 
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
}
