using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class TargetHandler : NetworkBehaviour
{
    private Transform target;
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

    public void SetTarget(Transform target) // 타겟 (재)설정
    {
        Debug.Log(gameObject.name + " target set");
        this.target = target;
        isAggro = true;

        // networkEnemyController.SetNavEnabled(true);
        StartCoroutine(ChaseStartCO());
    }

    IEnumerator ChaseStartCO()
    {
        yield return new WaitForSeconds(0.1f);
        networkEnemyController.SetIsChase(true); //single code에선 SetTarget에서 이미 호출함 그래서 효과없음.
        RPC_animatonSetBool("isWalk", true);
        Debug.Log(gameObject.name + " ChaseStart");
    }

    public void TargetisAlive() // 타겟 죽는거 확인
    {
        //Debug.Log(target.ToString());
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

    void TargetOff() // 타겟 해제
    {
        RPC_animatonSetBool("isWalk", false);
        RPC_animatonSetBool("isAttack", false);

        networkEnemyController.SetIsChase(false);
        agrroPulling.SetActive(true);
        isAggro = false;
        Debug.Log("TargetOff");
    }

    public Transform GetTarget()
    {
        return target;
    }
    public bool GetIsAggro()
    {
        return isAggro;
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
