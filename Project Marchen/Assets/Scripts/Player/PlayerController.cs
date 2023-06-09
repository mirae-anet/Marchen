using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;
    private CameraController camControl;
    private PlayerMain playerMain;
    private PlayerAttack playerAttack;

    private bool isMove = false;
    private bool isJump = false;
    private bool isDodge = false;
    private bool isAttack = false;
    private bool isReload = false;
    private bool isGrounded = false;

    private Vector3 moveDir;
    private Vector3 saveDir;

    // 입력값 저장 변수
    private Vector2 moveInput;
    private bool walkInput;
    private bool jumpInput;
    private bool dodgeInput;
    private bool attackInput;
    private bool reloadInput;

    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private Transform playerBody;

    [Header("설정")]
    public bool onVelo = true;
    [Range(1f, 30f)]
    public float moveSpeed = 20f;
    [Range(1f, 100f)]
    public float jumpPower = 30f;
    [Range(1f, 100f)]
    public float dodgePower = 50f;
    public Vector3 raySize = new Vector3(1.8f, 0.6f, 1.8f);

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        camControl = GetComponentInChildren<CameraController>();
        playerMain = GetComponent<PlayerMain>();
        playerAttack = GetComponent<PlayerAttack>();
    }
    
    void Update()
    {
        if (playerMain.GetIsDead())
        {
            rigid.velocity = Vector3.zero;
            return;
        }

        // 입력값 저장
        GetInput();

        // 바닥 체크
        GroundCheck();

        // 플레이어 조작
        PlayerMove();
        PlayerJump();
        PlayerDodge();

        PlayerAttack();
        PlayerReload();
    }

    private void GetInput()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // 이동 입력 벡터
        walkInput = Input.GetButton("Walk");
        jumpInput = Input.GetButtonDown("Jump");
        dodgeInput = Input.GetButtonDown("Dodge");

        attackInput = Input.GetButtonDown("Fire1");
        reloadInput = Input.GetButtonDown("Reload");
    }

    private void GroundCheck()
    {
        if (rigid.velocity.y > 0) // 추락이 아닐 때
            return;

        if (isGrounded)
        {
            isJump = false;
            anim.SetBool("isJump", false);
            //Debug.Log("착지");
        }
    }

    private void PlayerMove()
    {
        if (isDodge || playerMain.GetIsHit()) // 회피, 피격 중 이동 제한
            return;

        isMove = moveInput.magnitude != 0; // moveInput의 길이로 입력 판정
        
        if (isMove)
        {
            moveDir = camControl.GetMoveDir(moveInput);
            SetForward(moveDir);
            //playerBody.forward = lookForward; // 캐릭터 고정
            //playerBody.forward = moveDir;     // 카메라 고정

            float walkSpeed = (walkInput ? 0.3f : 1f); // 걷기면 속도 0.3배

            if (onVelo)
                rigid.velocity = new Vector3((moveDir * moveSpeed).x * walkSpeed, rigid.velocity.y, (moveDir * moveSpeed).z * walkSpeed); // 물리 이동
            else
                transform.position += moveDir * Time.deltaTime * moveSpeed * walkSpeed; // 절대 이동
        }
        else
            rigid.velocity = new Vector3(0f, rigid.velocity.y, 0f); // 미끄러짐 방지

        anim.SetBool("isRun", isMove);     // true일 때 걷는 애니메이션, false일 때 대기 애니메이션
        anim.SetBool("isWalk", walkInput); // isMove, walkOn 둘 다 True 일 때는 걷기
    }

    private void PlayerJump()
    {
        if (jumpInput && !isJump && !isDodge && !playerMain.GetIsHit() && !isAttack)
        {
            isJump = true;
            isGrounded = false;
            playerAttack.StopReload();

            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    private void PlayerDodge()
    {
        if (dodgeInput && isMove && !isDodge && !playerMain.GetIsHit() && !isAttack)
        {
            isDodge = true;
            playerAttack.StopReload();

            saveDir = moveDir; // 회피 방향 기억
            rigid.AddForce(saveDir.normalized * dodgePower, ForceMode.Impulse);

            anim.SetTrigger("doDodge");

            StartCoroutine(PlayerDodgeOut(0.5f));
        }
    }

    IEnumerator PlayerDodgeOut(float second)
    {
        yield return new WaitForSeconds(second);
        isDodge = false;
    }
    
    private void PlayerAttack()
    {
        if (attackInput && !isAttack && !isDodge && !playerMain.GetIsHit())
        {
            playerAttack.StopReload();
            SetIsAttack(true);

            rigid.velocity = new Vector3(0f, rigid.velocity.y, 0f);
            saveDir = camControl.gameObject.transform.forward; // 카메라 방향 기억
            saveDir.y = 0;

            playerAttack.DoAttack(saveDir); // 카메라 방향
        }
    }

    private void PlayerReload()
    {
        if (reloadInput && !GetActiveJDAH() && !isReload)
            playerAttack.DoReload();
    }

    public void SetIsGrounded(bool bol)
    {
        isGrounded = bol;
    }

    public void SetIsAttack(bool bol)
    {
        isAttack = bol;
    }

    public void SetIsReload(bool bol)
    {
        isReload = bol;
    }

    public bool GetActiveJDAH()
    {
        return isJump || isDodge || isAttack || playerMain.GetIsHit(); // 하나라도 작동하면 true
    }

    public void SetForward(Vector3 dir)
    {
        playerBody.forward = dir;
    }
}
