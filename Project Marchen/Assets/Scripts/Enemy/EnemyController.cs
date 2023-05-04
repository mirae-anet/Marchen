using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private EnemyMain enemyMain;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private Animator anim;
    private Transform target;

    private bool isChase = false;
    private bool isAttack = false;
    private bool isAggro = false;

    private int moveDir = 0;
    private int isMove = 0;

    [Header("오브젝트 연결")]   
    [SerializeField]
    private BoxCollider meleeArea;
    [SerializeField]
    private GameObject bullet;

    [Header("설정")]
    public float moveDis = 3f;
    public float moveSpeed = 5f;

    void Awake()
    {
        enemyMain = GetComponent<EnemyMain>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        StartCoroutine(Think(Random.Range(0.5f, 4f))); // 논어그로
    }

    void FixedUpdate()
    {
        if (!isAggro) // 논어그로
        {
            EnemyMove();
        }
        else // 어그로 풀링
        {
            FreezeVelocity();
            EnemyChase();
            Aiming();
        }
    }

    void EnemyMove()
    {
        rigid.velocity = transform.forward * moveSpeed * isMove;
    }

    IEnumerator Think(float worry)
    {
        yield return new WaitForSeconds(worry);     // 고민
        moveDir = Random.Range(0, 360);             // 랜덤 방향 이동
        transform.Rotate(0, moveDir, 0);
        isMove = 1;
        anim.SetBool("isWalk", true);

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤
        anim.SetBool("isWalk", false);

        yield return new WaitForSeconds(worry);     // 고민
        moveDir = -180;                             // 되돌아감
        transform.Rotate(0, moveDir, 0);
        isMove = 1;
        anim.SetBool("isWalk", true);

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤
        anim.SetBool("isWalk", false);

        StartCoroutine(Think(Random.Range(0.5f, 4f)));
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void EnemyChase()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void Aiming()
    {
        float targetRadius = 0;
        float targetRange = 0;

        switch (enemyMain.getEnemyType())
        {
            case EnemyMain.Type.Melee:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;

            case EnemyMain.Type.Range:
                targetRadius = 0.5f;
                targetRange = 25f;
                break;
        }

        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position,           // 위치
                                  targetRadius,                 // 반지름
                                  transform.forward,            // 방향
                                  targetRange,                  // 방향으로 부터 거리
                                  LayerMask.GetMask("Player")); // 레이어 특정

        if (rayHits.Length > 0 && !isAttack)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyMain.getEnemyType())
        {
            case EnemyMain.Type.Melee:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;

            case EnemyMain.Type.Range:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, new Vector3(transform.position.x, transform.position.y + 2.3f, transform.position.z), transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    IEnumerator ChaseStart()
    {
        yield return new WaitForSeconds(0.5f);
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
        isAggro = true;

        StopAllCoroutines();
        StartCoroutine(ChaseStart());
    }

    public void setIsChase(bool bol)
    {
        isChase = bol;
        nav.enabled = bol;
    }
}
