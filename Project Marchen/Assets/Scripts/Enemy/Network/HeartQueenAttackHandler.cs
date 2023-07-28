using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @brief 1 stage의 boss인 HeartQueen의 공격과 관련된 클래스
public class HeartQueenAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;

    /// @brief 에너미의 공격이 취소될 수 있는지 설정. default는 false.
    public bool attackCancel = false;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (반지름)
    public float targetRadius =  3f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (최대 거리)
    public float targetRange = 30f;
    public Transform detectionPos;
    private Vector3 aimVec;
    
    [Header("오브젝트 연결")]
    // public BulletHandler bulletPrefab;
    public GuidedBulletHandler guidedBullet; // 유도탄
    public StraightBulletHandler straightBullet;
    public BulletHandler areaBullet;
    public Transform gBulletPortL;
    public Transform gBulletPortR;
    public Transform sBulletPort;

    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    private TargetHandler targetHandler;
    public AudioSource attackGuidedSound;
    public AudioSource attackStraightSound;
    public AudioSource attackAreaSound;

    void Start()
    {
        networkEnemyController = GetComponent<NetworkEnemyController>();
        anim = GetComponentInChildren<Animator>();
        enemyHPHandler = GetComponent<EnemyHPHandler>();
        targetHandler = GetComponent<TargetHandler>();
    }

    /// @brief 타겟을 향해서 공격을 준비.
    /// @details 레이캐스트로 타겟을 탐색. hit 시 AttackThick를 호출.
    public override void Aiming() 
    {
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
                    StartCoroutine("AttackThink");
                    break;
                }
            }
        }
    }

    /// @brief 공격 패턴을 선택.
    /// @details 유도 공격 40%, 직선 공격 40%, 전방향 공격 20% 확률. 
    IEnumerator AttackThink()
    {
        networkEnemyController.SetIsChase(false);
        isAttack = true;

        yield return new WaitForSeconds(0.5f);
        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:

            case 1:
                StartCoroutine(AttackGuided()); // 유도탄 발사
                break;

            case 2:

            case 3:
                StartCoroutine(AttackStraight()); // 차지(직선)탄 발사
                break;

            case 4:
                StartCoroutine(AttackArea()); // 전체 공격(16개)
                break;
        }
    }

    /// @brief 유도 공격.
    /// @see GuidedBulletHandler
    IEnumerator AttackGuided()
    {
        yield return new WaitForSeconds(0.3f);
        RPC_animatonSetBool("isAttackGuided", true);

        yield return new WaitForSeconds(0.3f);
        Runner.Spawn(guidedBullet, gBulletPortL.position, gBulletPortL.rotation, Object.StateAuthority, (runner, spawnedBulletL) =>
        {
            if(spawnedBulletL.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
                navMeshAgent.Warp(gBulletPortL.position);

            GuidedBulletHandler guidedBulletHandler = spawnedBulletL.GetComponent<GuidedBulletHandler>();
            guidedBulletHandler.Fire(Object.StateAuthority, Object, transform.name);
            guidedBulletHandler.setTarget(targetHandler.GetTarget());
        });
        RPC_AudioPlay("guided");

        yield return new WaitForSeconds(0.5f);
        RPC_animatonSetBool("isAttackGuided2", true);

        yield return new WaitForSeconds(0.3f);
        Runner.Spawn(guidedBullet, gBulletPortR.position, gBulletPortR.rotation, Object.StateAuthority, (runner, spawnedBulletR) =>
        {
            if(spawnedBulletR.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
                navMeshAgent.Warp(gBulletPortR.position);

            GuidedBulletHandler guidedBulletHandler = spawnedBulletR.GetComponent<GuidedBulletHandler>();
            guidedBulletHandler.Fire(Object.StateAuthority, Object, transform.name);
            guidedBulletHandler.setTarget(targetHandler.GetTarget());
        });
        RPC_AudioPlay("guided");

        yield return new WaitForSeconds(1f);
        RPC_animatonSetBool("isAttackGuided", false);
        RPC_animatonSetBool("isAttackGuided2", false);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
    }

    /// @brief 직선 공격.
    /// @see StraightBulletHandler
    IEnumerator AttackStraight()
    {
        RPC_animatonSetBool("isAttackStraight", true);

        yield return new WaitForSeconds(0.8f);
        // GameObject instantBullet = Instantiate(straightBullet, sBulletPort.position, sBulletPort.rotation);
        Runner.Spawn(straightBullet, sBulletPort.position, sBulletPort.rotation, Object.StateAuthority, (runner, spawnedBullet) =>
        {
            StraightBulletHandler straightBulletHandler = spawnedBullet.GetComponent<StraightBulletHandler>();
            straightBulletHandler.Fire(Object.StateAuthority, Object, transform.name);
        });
        RPC_AudioPlay("straight");

        yield return new WaitForSeconds(1.5f);

        RPC_animatonSetBool("isAttackStraight", false);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
    }

    /// @brief 전방향 공격.
    /// @see AreaBulletHandler
    IEnumerator AttackArea()
    {
        RPC_animatonSetBool("isAttackArea", true);

        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 17; i++)
        {
            Runner.Spawn(areaBullet, transform.position, Quaternion.Euler(0f, 22.5f * i, 0f), Object.StateAuthority, (runner, spawnedBullet) =>
            {
                BulletHandler areaBulletHandler = spawnedBullet.GetComponent<BulletHandler>();
                areaBulletHandler.Fire(Object.StateAuthority, Object, transform.name);
            });
        }
        RPC_AudioPlay("area");

        yield return new WaitForSeconds(1f);
        RPC_animatonSetBool("isAttackArea", false);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
    }

    // --------------------------------------------------
    /// @brief 공격 취소.
    public override void AttackCancel()
    {
        if (!attackCancel)
            return;

        networkEnemyController.SetIsChase(true);
        isAttack = false;
        RPC_animatonSetBool("isAttackGuided", false);
        RPC_animatonSetBool("isAttackGuided2", false);
        RPC_animatonSetBool("isAttackStraight", false);
        RPC_animatonSetBool("isAttackArea", false);
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
        switch (audioType)
        {
            case "guided":
                attackGuidedSound.Play();
                break;

            case "straight":
                attackStraightSound.Play();
                break;

            case "area":
                attackAreaSound.Play();
                break;
        }
    }
}
