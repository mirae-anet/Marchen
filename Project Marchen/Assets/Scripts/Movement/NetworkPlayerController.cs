using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;

public class NetworkPlayerController : NetworkBehaviour
{

    private bool isMove = false;
    private bool isJump;
    private bool isDodge;

    private Vector3 moveDir;
    private Vector3 dodgeDir;
    private Vector3 feetpos;

    // 입력값 저장 변수
    // private Vector2 moveInput;
    private bool walkInput;
    private bool jumpInput;
    private bool dodgeInput;

    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private Transform playerPF;
    [SerializeField]
    private Transform playerBody;
    // [SerializeField]
    // private Transform playerModel;

    [Header("설정")]
    // public bool onVelo = true;
    [Range(1f, 30f)]
    public float moveSpeed = 20f;
    [Range(1f, 100f)]
    public float jumpPower = 30f;
    [Range(1f, 100f)]
    public float dodgePower = 50f;
    [Range(0.0f, 1f)]
    public float doubleTapTime = 0.2f;
    public Vector3 raySize = new Vector3(1.8f, 0.6f, 1.8f);
    
    //other component
    private Animator anim;
    private Rigidbody rigid;
    private HPHandler hpHandler;
    private CharacterMovementHandler characterMovementHandler;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        hpHandler = GetComponent<HPHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }
    public override void FixedUpdateNetwork()
    {
        if(hpHandler.isDead)
            return;
        
        GroundCheck(); // 바닥 체크 후 anim.SetBool("isJump", false);

        if(Object.HasInputAuthority)
        {
            // anim.SetBool("isJump", isJump);
            RPC_animatonSetBool("isJump", isJump);
        }

        if(GetInput(out NetworkInputData networkInputData))
        {
            if(!characterMovementHandler.GetCharacterControllerEnabled())
                return;

            // 입력값 저장
            SetInput(networkInputData);

            // 플레이어 조작
            PlayerMove();
            PlayerJump();
            PlayerDodge();
        }
    }

    public void GroundCheck()
    {
        if (rigid.velocity.y > 0) // 추락이 아닐 때
            return;

        feetpos = new Vector3(playerBody.position.x, playerBody.position.y+0.3f, playerBody.position.z);

        //local화 가능? 가능할 듯 update를 이용해서 hasInputAuthority확인 후에 계산하고 networked로 동기화하기
        if (Physics.BoxCast(feetpos, raySize / 2, Vector3.down, out RaycastHit rayHit, Quaternion.identity, 1f, LayerMask.GetMask("Ground")))
        {
            if (rayHit.distance < 1.0f)
            {
                isJump = false;
                // if(Object.HasInputAuthority)
                    // anim.SetBool("isJump", false);
                // anim.SetBool("isJump", false);
                // RPC_animatonSetBool("isJump", false);
                //Debug.Log("착지");
            }
            else
            {
                isJump = true;
                // if(Object.HasInputAuthority)
                //     anim.SetBool("isJump", true);
            }
        }
    }

    public void SetInput(NetworkInputData networkInputData)
    {
        this.isMove = networkInputData.isMove;
        this.moveDir = networkInputData.moveDir;
        this.walkInput = networkInputData.walkInput;
        this.jumpInput = networkInputData.jumpInput;
        this.dodgeInput = networkInputData.dodgeInput;
    }

    public void PlayerMove()
    {
        if (isDodge && !hpHandler.getIsHit()) // 회피, 피격 중 이동 제한
            return;
        
        if(Object.HasStateAuthority)
        {
            if (isMove)
            {
                RPC_LookForward(moveDir);

                float walkSpeed = (walkInput ? 0.3f : 1f); // 걷기면 속도 0.3배

                rigid.velocity = new Vector3((moveDir * moveSpeed).x * walkSpeed, rigid.velocity.y, (moveDir * moveSpeed).z * walkSpeed); // 물리 이동
            }
            else
                rigid.velocity = new Vector3(0f, rigid.velocity.y, 0f); // 미끄러짐 방지
        }

        if(Object.HasInputAuthority)
        {
            RPC_animatonSetBool("isRun", isMove);
            RPC_animatonSetBool("isWalk", walkInput);
            // anim.SetBool("isRun", isMove);     // true일 때 걷는 애니메이션, false일 때 대기 애니메이션
            // anim.SetBool("isWalk", walkInput); // isMove, walkOn 둘 다 True 일 때는 걷기
        }
    }

    public void PlayerJump()
    {
        if (jumpInput && !isJump && !isDodge && !hpHandler.getIsHit())
        {

            isJump = true;

            if(Object.HasStateAuthority)
            {
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }

            if(Object.HasInputAuthority)
            {
                RPC_animatonSetBool("isJump", true);
                RPC_animatonSetTrigger("doJump");
                // anim.SetBool("isJump", true);
                // anim.SetTrigger("doJump");
            }

        }
    }

    public void PlayerDodge()
    {
        if (dodgeInput && isMove && !isDodge && !hpHandler.getIsHit())
        {
            if(Object.HasStateAuthority)
            {
                dodgeDir = moveDir; // 회피 방향 기억
                rigid.AddForce(dodgeDir.normalized * dodgePower, ForceMode.Impulse);
            }

            if(Object.HasInputAuthority)
                RPC_animatonSetTrigger("doDodge");
                // anim.SetTrigger("doDodge");

            isDodge = true;
            StartCoroutine(PlayerDodgeOut(0.5f));
        }
    }

    IEnumerator PlayerDodgeOut(float second)
    {
        yield return new WaitForSeconds(second);
        isDodge = false;
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LookForward(Vector3 moveDir)
    {
        playerPF.forward = moveDir;
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
        // anim.SetTrigger("doJump");
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }

    /*
    private void OnDrawGizmos()
    {
        feetpos = new Vector3(playerBody.position.x, playerBody.position.y, playerBody.position.z);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(feetpos, raySize);
    }
    */

}
