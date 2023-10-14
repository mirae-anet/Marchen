using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoController : MonoBehaviour
{
    // 인스펙터
    [Header("설정")]
    [SerializeField]
    private float moveSpeed = 50f;
    [SerializeField]
    private float moveDis = 10f;

    private int isMove = 0;
    private int moveDir = 0;

    void Start()
    {
        StartCoroutine("Think", (Random.Range(0.5f, 4f))); // 논어그로
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move() // 어그로 아닐 때 이동
    {
        transform.position += transform.forward * moveSpeed * isMove * Time.deltaTime;
        //rigid.velocity = transform.forward * moveSpeed * isMove; // moveSpeed는 지정, isMove는 Think()에서 결정
    }

    IEnumerator Think(float worry) // 어그로 아닐 때 이동 결정하는 함수
    {
        yield return new WaitForSeconds(worry);     // 고민
        moveDir = Random.Range(0, 360);             // 랜덤 방향 이동
        transform.Rotate(0, moveDir, 0);
        isMove = 1;

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤

        yield return new WaitForSeconds(worry);     // 고민
        moveDir = -180;                             // 되돌아감
        transform.Rotate(0, moveDir, 0);
        isMove = 1;

        yield return new WaitForSeconds(moveDis);   // 일정 거리 까지
        isMove = 0;                                 // 멈춤

        StartCoroutine("Think", (Random.Range(0.5f, 4f)));
    }
}
