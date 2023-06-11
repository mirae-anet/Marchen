using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class HeartQueenAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;
    public bool attackCancel = false;
    public float targetRadius =  3f;
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

    public override void Aiming() // 레이캐스트로 플레이어 위치 특정
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

    IEnumerator AttackArea()
    {
        RPC_animatonSetBool("isAttackArea", true);

        yield return new WaitForSeconds(0.5f);
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
    public override void AttackCancel()
    {
        if (!attackCancel)
            return;

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
