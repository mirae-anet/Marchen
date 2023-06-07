using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    private EnemyMain enemyMain;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private Animator anim;
    private BoxCollider boxCollider;

    private Vector3 lookVec;
    private Vector3 tauntVec;

    private bool isLook = false;
    private bool isChase = false;

    public Transform target;

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

    void Awake()
    {
        enemyMain = GetComponent<EnemyMain>();
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        anim = GetComponentInChildren<Animator>();
        nav = GetComponent<NavMeshAgent>();

        nav.isStopped = true;
    }

    void Start()
    {
        StartCoroutine(Think());
    }

    void Update()
    {
        if (enemyMain.GetIsDead())
        {
            StopAllCoroutines();
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec);
    }

    void FixedUpdate()
    {
        FreezeVelocity();
    }

    void FreezeVelocity()
    {
        if (!isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);
        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:

            case 1:
                StartCoroutine(MissileShot()); // 미사일 발사 패턴
                break;

            case 2:

            case 3:
                StartCoroutine(RockShot()); // 돌 굴러가는 패턴
                break;

            case 4:
                StartCoroutine(Taunt()); // 점프 공격 패턴
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BulletBoss bossMissileA = instantMissileA.GetComponent<BulletBoss>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BulletBoss bossMissileB = instantMissileB.GetComponent<BulletBoss>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }
    
    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");

        Instantiate(bullet, transform.position, transform.rotation);

        yield return new WaitForSeconds(3f);
        isLook = true;

        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        if (meleeArea != null)
            meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        if (meleeArea != null)
            meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        boxCollider.enabled = true;
        nav.isStopped = true;
        
        StartCoroutine(Think());
    }
}