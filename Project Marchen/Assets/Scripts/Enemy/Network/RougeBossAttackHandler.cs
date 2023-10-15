using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @brief 1 stage의 boss인 HeartQueen의 공격과 관련된 클래스
public class RougeBossAttackHandler : EnemyAttackHandler
{
    private bool isAttack = false;

    /// @brief 에너미의 공격이 취소될 수 있는지 설정. default는 false.
    public bool attackCancel = false;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (반지름)
    public float targetRadius =  5f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (최대 거리)
    public float targetRange = 5f;
    /// @brief 타겟이 해당 범위에 들어오면 공격을 수행. (가로 세로 높이)
    public Vector3 boxSize = new Vector3(6f, 10f, 8f);
    public Transform detectionPos;
    /// @brief 입히는 피해량
    public int damageAmount = 50;
    private Vector3 aimVec;
    
    [Header("오브젝트 연결")]
    public Transform BulletPort;
    public Transform anchorPoint;
    public BulletHandler SnakePrefab;

    //other component
    NetworkEnemyController networkEnemyController;
    private Animator anim;
    private EnemyHPHandler enemyHPHandler;
    private TargetHandler targetHandler;
    public AudioSource attackSwingSound;
    public AudioSource attackBlowSound;

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
    /// @details 일반 휘두르기 40%, 돌진 휘두르기 40%, 뱀 발사 20% 확률. 
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
                StartCoroutine(AttackAround()); // 일반 휘두르기 
                break;

            case 2:

            case 3:
                StartCoroutine(AttackDash()); // 돌진 휘두르기 
                break;

            case 4:
                StartCoroutine(AttackThrow()); // 뱀 발사 
                break;
        }
    }

    /// @brief 일반 휘두르기 공격.
    IEnumerator AttackAround(){
        yield return new WaitForSeconds(0.1f);
        RPC_animatonSetBool("isAttackAround", true);
        RPC_AudioPlay("swing");

        yield return new WaitForSeconds(0.7f);
        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        float endTime = Time.time + 1.0f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(anchorPoint.transform.forward), Object.StateAuthority, hits, LayerMask.GetMask("PlayerHitBox"));
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
        RPC_AudioPlay("blow");

        yield return new WaitForSeconds(0.3f);
        RPC_animatonSetBool("isAttackAround", false);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
    }

    /// @brief 돌진 휘두르기.
    IEnumerator AttackDash()
    {
        //dash
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("isAttackDash", true);
        RPC_AudioPlay("swing");
        //melee area on
        yield return new WaitForSeconds(0.7f);
        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        float endTime = Time.time + 1.5f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(anchorPoint.transform.forward), Object.StateAuthority, hits, LayerMask.GetMask("PlayerHitBox"));
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
        RPC_AudioPlay("blow");

        yield return new WaitForSeconds(0.3f);
        RPC_animatonSetBool("isAttackDash", false);

        networkEnemyController.SetIsChase(true);
        isAttack = false;
    }

    /// @brief 뱀 투척.
    IEnumerator AttackThrow()
    {
        RPC_animatonSetBool("isAttackThrow", true);

        yield return new WaitForSeconds(1f);
        // spawn snake
        Runner.Spawn(SnakePrefab, anchorPoint.position, Quaternion.LookRotation(aimVec.normalized), Object.StateAuthority, (runner, spawnedBullet) =>
        {
            spawnedBullet.GetComponent<BulletHandler>().Fire(Object.StateAuthority, Object, transform.name);
        });

        yield return new WaitForSeconds(0.5f);
        RPC_animatonSetBool("isAttackThrow", false);

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
        RPC_animatonSetBool("isAttackSlash", false);
        RPC_animatonSetBool("isAttackShot", false);
        RPC_animatonSetBool("isAttackDash", false);
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
            case "swing":
                attackSwingSound.Play();
                break;

            case "blow":
                attackBlowSound.Play();
                break;
        }
    }
}
