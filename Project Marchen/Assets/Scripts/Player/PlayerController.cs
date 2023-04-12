using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;
    private CameraController camControl;

    private bool walkOn;  // 걷기
    private Vector3 moveDir;

    [Header("오브젝트 연결")]
    [SerializeField]
    private Transform playerBody;

    [Header("설정")]
    [Range(1f, 30f)]
    public float moveSpeed = 15f;

    public bool onVelo = true;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        camControl = GetComponentInChildren<CameraController>();
    }
    
    void Update()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));    // 이동 입력 값으로 moveInput 벡터
        bool isMove = moveInput.magnitude != 0; // moveInput의 길이로 입력 판정
        walkOn = Input.GetButton("Walk");
        
        if (isMove)
        {
            moveDir = camControl.getMoveDir(moveInput);

            //playerBody.forward = lookForward;  // 캐릭터 고정
            playerBody.forward = moveDir;        // 카메라 고정

            float walkSpeed = (walkOn ? 0.3f : 1f);

            if (onVelo)
                rigid.velocity = new Vector3((moveDir * moveSpeed).x * walkSpeed, rigid.velocity.y, (moveDir * moveSpeed).z * walkSpeed); // 물리 이동
            else
                transform.position += moveDir * Time.deltaTime * moveSpeed * walkSpeed; // 절대 이동
        }

        anim.SetBool("isRun", isMove); // true일 때 걷는 애니메이션, false일 때 대기 애니메이션
        anim.SetBool("isWalk", walkOn); // 최종적으로는 isMove, walkOn 둘 다 True 일 때 걷기
    }


}
