using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    private BossMain bossMain;
    private NavMeshAgent nav;
    private Animator anim;
    private Transform target;
    
    private bool isChase = false;
    private bool isHit = false;
    private bool isAttack = false;
    private bool isAggro = false;

    [Header("오브젝트 연결")]
    [SerializeField]
    public GameObject guidedBullet; // 유도탄
    [SerializeField]
    private GameObject straightBullet; // 차지탄
    [SerializeField]
    private GameObject AreaBullet; // 전체공격
    [SerializeField]
    private Transform gBulletPortL;
    [SerializeField]
    private Transform gBulletPortR;
    [SerializeField]
    private Transform sBulletPort;
    [SerializeField]
    private GameObject agrroPulling;

    [Header("설정")]
    [Range(0f, 10f)]
    public float targetRadius = 5f;
    [Range(1f, 100f)]
    public float targetRange = 30f; // 공격 사정 거리
    [SerializeField]
    private bool attackCancel = false; // 피격 시 공격 멈출지

    void Awake()
    {
        bossMain = GetComponent<BossMain>();
        anim = GetComponentInChildren<Animator>();
        nav = GetComponent<NavMeshAgent>();
    }

    void FixedUpdate()
    {
        if (bossMain.GetIsDead())
        {
            StopAllCoroutines();
            return;
        }

        if (isAggro) // 어그로 풀링
        {
            //FreezeVelocity();
            TargetisAlive();
            EnemyChase();
            Aiming();
            AttackCancel();
        }
    }

    // --------------------------- 타겟 관련 ------------------------
    public void SetTarget(Transform transform) // 타겟 (재)설정
    {
        target = transform;
        isAggro = true;

        SetIsNavEnabled(true);

        Debug.Log(gameObject.name + " target reset");
        StartCoroutine(ChaseStart());
    }

    void TargetOff() // 타겟 해제
    {
        //anim.SetBool("isAttackGuided", false);
        //anim.SetBool("isAttackStraight", false);
        //anim.SetBool("isAttackArea", false);
        //anim.SetBool("isWalk", false);

        SetIsNavEnabled(false);
        agrroPulling.SetActive(true);
        isAggro = false;
    }

    // --------------------------- 어그로 풀링 ------------------------
    IEnumerator ChaseStart()
    {
        yield return new WaitForSeconds(0.8f);
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void TargetisAlive() // 타겟 죽는거 확인
    {
        //Debug.Log(target.ToString());
        if (target == null) // 타겟이 없으면
        {
            TargetOff();
            return;
        }
        else if (target.gameObject.GetComponent<PlayerMain>().GetIsDead()) // 타겟이 죽으면
        {
            TargetOff();
            return;
        }
        else // 타겟이 있고 살아 있다면
            return;

        //Debug.Log(target.parent.gameObject.ToString());
    }

    void EnemyChase()
    {
        if (!nav.enabled) // 네비를 끄면
            return;

        nav.SetDestination(target.position);
        nav.isStopped = !isChase || isHit; // 추적 중이 아닐 때, 피격일 때) 네비 멈춤
    }

    void Aiming() // 레이캐스트로 플레이어 위치 특정
    {
        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position,           // 위치
                                  targetRadius,                 // 반지름
                                  transform.forward,            // 방향
                                  targetRange,                  // 방향으로 부터 거리 (공격 사정 거리)
                                  LayerMask.GetMask("Player")); // 레이어 특정

        if (rayHits.Length > 0 && !isAttack && !isHit)
            StartCoroutine("AttackThink");
    }

    IEnumerator AttackThink()
    {
        isChase = false;
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
        isChase = false;
        isAttack = true;

        yield return new WaitForSeconds(0.3f);
        anim.SetBool("isAttackGuided", true);

        yield return new WaitForSeconds(0.3f);
        GameObject instantBulletA = Instantiate(guidedBullet, gBulletPortL.position, gBulletPortL.rotation);
        GuidedBullet bossBulletA = instantBulletA.GetComponent<GuidedBullet>();
        bossBulletA.setTarget(target);

        yield return new WaitForSeconds(0.5f);
        anim.SetBool("isAttackGuided2", true);

        yield return new WaitForSeconds(0.3f);
        GameObject instantBulletB = Instantiate(guidedBullet, gBulletPortR.position, gBulletPortR.rotation);
        GuidedBullet bossBulletB = instantBulletB.GetComponent<GuidedBullet>();
        bossBulletB.setTarget(target);

        yield return new WaitForSeconds(1f);
        anim.SetBool("isAttackGuided", false);
        anim.SetBool("isAttackGuided2", false);

        isChase = true;
        isAttack = false;
    }
    
    IEnumerator AttackStraight()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttackStraight", true);

        yield return new WaitForSeconds(0.8f);
        GameObject instantBullet = Instantiate(straightBullet, sBulletPort.position, sBulletPort.rotation);

        yield return new WaitForSeconds(0.5f);
        instantBullet.GetComponent<CapsuleCollider>().enabled = true;

        yield return new WaitForSeconds(1f);
        Destroy(instantBullet);

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttackStraight", false);
    }

    IEnumerator AttackArea()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttackArea", true);

        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 17; i++)
        {
            GameObject instantBullet = Instantiate(AreaBullet, transform.position, Quaternion.Euler(0f, 22.5f * i, 0f));
            Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
            rigidBullet.velocity = instantBullet.transform.forward * 20;
        }

        yield return new WaitForSeconds(1f);
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttackArea", false);
    }

    void AttackCancel()
    {
        if (!attackCancel)
            return;

        if (isHit)
        {
            StopCoroutine("Attack");

            isChase = true;
            isAttack = false;

            anim.SetBool("isAttackArea", false);
            anim.SetBool("isAttackStraight", false);
            anim.SetBool("isAttackGuided", false);
            anim.SetBool("isAttackGuided2", false);
        }
    }

    // --------------------------- 외부 참조 함수 ------------------------
    public void SetIsNavEnabled(bool bol)
    {
        isChase = bol;
        nav.enabled = bol;
    }

    public void SetIsChase(bool bol)
    {
        isChase = bol;
    }

    public void setIsHit(bool bol)
    {
        isHit = bol;
    }
}