using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    private EnemyMain enemyMain;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private Animator anim;

    private bool isChase;
    private bool isAttack;

    [Header("오브젝트 연결")]
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;

    void Awake()
    {
        enemyMain = GetComponent<EnemyMain>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        float targetRadius = 0;
        float targetRange = 0;

        switch (enemyMain.getEnemyType())
        {
            case EnemyMain.Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;

            case EnemyMain.Type.B:
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

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyMain.getEnemyType())
        {
            case EnemyMain.Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;

            case EnemyMain.Type.B:
                yield return new WaitForSeconds(0.5f);
                //GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
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

    public void setIsChase(bool bol)
    {
        isChase = bol;
    }

    public void setNavEnabled(bool bol)
    {
        nav.enabled = bol;
    }
}
