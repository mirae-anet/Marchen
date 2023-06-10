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
    private BoxCollider meleeArea;
    [SerializeField]
    private GameObject bullet; // 바위
    [SerializeField]
    public GameObject missile; // 미사일
    [SerializeField]
    private Transform missilePortA;
    [SerializeField]
    private Transform missilePortB;
    [SerializeField]
    private GameObject agrroPulling;

    [Header("설정")]
    [Range(0f, 10f)]
    public float targetRadius = 5f;
    [Range(1f, 100f)]
    public float targetRange = 55f; // 공격 사정 거리
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
        anim.SetBool("isAttackGuided", false);
        anim.SetBool("isAttackStraight", false);
        anim.SetBool("isAttackArea", false);
        anim.SetBool("isWalk", false);

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

        yield return new WaitForSeconds(0.1f);
        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:

            case 1:
                StartCoroutine(AttackGuided()); // 미사일 발사 패턴
                break;

            case 2:

            case 3:
                StartCoroutine(AttackStraigh()); // 돌 굴러가는 패턴
                break;

            case 4:
                StartCoroutine(AttackArea()); // 점프 공격 패턴
                break;
        }
    }

    IEnumerator AttackGuided()
    {
        isChase = false;
        isAttack = true;
        //yield return new WaitForSeconds(0.3f);
        anim.SetBool("isAttackGuided", true);

        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        GuidedBullet bossMissileA = instantMissileA.GetComponent<GuidedBullet>();
        bossMissileA.setTarget(target);

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        GuidedBullet bossMissileB = instantMissileB.GetComponent<GuidedBullet>();
        bossMissileB.setTarget(target);

        yield return new WaitForSeconds(2f);
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttackGuided", false);
    }
    
    IEnumerator AttackStraigh()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttackStraight", true);

        GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
        Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
        rigidBullet.velocity = transform.forward * 20;

        yield return new WaitForSeconds(3f);
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttackStraight", false);
    }

    IEnumerator AttackArea()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttackArea", true);

        yield return new WaitForSeconds(1.5f);
        if (meleeArea != null)
            meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        if (meleeArea != null)
            meleeArea.enabled = false;

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

            anim.SetBool("isAttack", false);

            if (meleeArea != null)
                meleeArea.enabled = false;
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