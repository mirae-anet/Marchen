using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkEnemyController : NetworkBehaviour
{
    private bool isThinking = false;
    private int isMove = 0;
    private bool isAggro = false;

    [Header("설정")]
    public float moveDis = 3f;
    public float moveSpeed = 5f;
    public bool attackCancel = true;

    private int moveDir = 0;

    //other component
    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    void Start()
    {
        // if(!Object.HasStateAuthority)
        //     StartCoroutine("Think", (Random.Range(0.5f, 4f))); // 논어그로
    }


    private void FixedUpdate() 
    {
        if(!Object.HasStateAuthority)
            return;

        // if (enemyMain.GetIsDead())
        //     return;

        if (!isAggro) // 논어그로
        {
            if(!isThinking)
            {
                StartCoroutine("ThinkCO", (Random.Range(0.5f, 4f)));
                isThinking = true;
            }
            EnemyWander();
        }
        else
        {
            StopCoroutine("ThinkCO");
        }
    }


    void EnemyWander() // 어그로 아닐 때 이동
    {
        transform.position += transform.forward* moveSpeed * isMove * Time.deltaTime;
    }

    IEnumerator ThinkCO(float worry) // 어그로 아닐 때 이동 결정하는 함수
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

        isThinking = false;
    }
}
