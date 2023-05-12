using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public bool isLook;

    public GameObject missile;
    public Transform target;
    public Transform missilePortA;
    public Transform missilePortB;

    private bool isChase = false;

    private EnemyMain enemyMain;
    private Rigidbody rigid;
    private BoxCollider boxCollider;
    private NavMeshAgent nav;
    private Animator anim;
    private Vector3 lookVec;
    private Vector3 tauntVec;

    [Header("오브젝트 연결")]
    [SerializeField]
    private BoxCollider meleeArea;
    [SerializeField]
    private GameObject bullet;

    void Awake()
    {
        enemyMain = GetComponent<EnemyMain>();
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
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
                // 미사일 발사 패턴
                StartCoroutine(MissileShot());

                break;

            case 2:
            case 3:
                // 돌 굴러가는 패턴
                StartCoroutine(RockShot());

                break;

            case 4:
                // 점프 공격 패턴
                StartCoroutine(Taunt());

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

        if (boxCollider != null && meleeArea != null)
            meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);

        if (boxCollider != null && meleeArea != null)
            meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);

        if (boxCollider != null)
            boxCollider.enabled = true;

        isLook = true;
        nav.isStopped = true;
        
        StartCoroutine(Think());
    }
}