using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class NetworkPlayerController : NetworkBehaviour
{

    private bool isControllerEnable = true;
    private bool isMove = false;
    private bool isJump;
    private bool isDodge;
    private bool isAttack = false;
    private bool isReload = false;
    private bool isInteract = false;
    private bool attackInput;
    private bool reloadInput;

    private Vector3 moveDir;
    private Vector3 dodgeDir;
    private Vector3 feetpos;
    public Vector3 aimForwardVector;

    // 입력값 저장 변수
    // private Vector2 moveInput;
    private bool walkInput;
    private bool jumpInput;
    private bool dodgeInput;
    private bool interactInput;

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
    private AttackHandler attackHandler;
    private NetworkInGameMessages networkInGameMessages;
    private PlayerActionHandler playerActionHandler;

    //패널
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        hpHandler = GetComponent<HPHandler>();
        attackHandler = GetComponent<AttackHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        playerActionHandler = GetComponent<PlayerActionHandler>();
    }
    public override void FixedUpdateNetwork()
    {
        if (hpHandler.GetIsDead())
            return;
        
        GroundCheck(); // 바닥 체크 후 anim.SetBool("isJump", false);
        // Debug.Log($"{isJump}");

        if(Object.HasInputAuthority)
        {
            RPC_animatonSetBool("isJump", isJump);
        }

        if(GetInput(out NetworkInputData networkInputData))
        {
            if(!isControllerEnable)
                return;
            if(networkInGameMessages.isTyping)
            {
                if(Object.HasStateAuthority)
                    rigid.velocity = new Vector3(0f, rigid.velocity.y, 0f);
                if(Object.HasInputAuthority)
                    RPC_animatonSetBool("isRun", false);
                return;
            }
            
            // 입력값 저장
            SetInput(networkInputData);

            // 플레이어 조작

            PlayerMove();
            PlayerJump();
            PlayerDodge();
            PlayerAttack();
            PlayerReload();
            PlayerInteract();
        }
    }

    public void GroundCheck()
    {
        if (rigid.velocity.y > 0) // 추락이 아닐 때
            return;
        if (isDodge)
            return;
        // if (isAttack)
        //     return;

        feetpos = new Vector3(playerBody.position.x, playerBody.position.y, playerBody.position.z);

        Collider[] colliders = Physics.OverlapBox(feetpos, raySize/2, Quaternion.LookRotation(transform.forward), LayerMask.GetMask("Ground"));

        if(colliders.Length > 0)
        {
            isJump = false;
        }
        else
        {
            isJump = true;    
        }
    }

    public void SetInput(NetworkInputData networkInputData)
    {
        this.isMove = networkInputData.isMove;
        this.moveDir = networkInputData.moveDir;
        this.walkInput = networkInputData.walkInput;
        this.jumpInput = networkInputData.jumpInput;
        this.dodgeInput = networkInputData.dodgeInput;
        this.attackInput = networkInputData.attackInput;
        this.reloadInput = networkInputData.reloadInput;
        this.aimForwardVector = networkInputData.aimForwardVector;
        this.interactInput = networkInputData.interactInput;
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
        }
    }

    public void PlayerJump()
    {
        if (jumpInput && !isJump && !isDodge && !isAttack && !hpHandler.getIsHit())
        {

            isJump = true;

            if(Object.HasStateAuthority)
            {
                attackHandler.StopReload();
                rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
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
        if (dodgeInput && isMove && !isDodge && !isAttack && !hpHandler.getIsHit())
        {
            if(Object.HasStateAuthority)
            {
                attackHandler.StopReload();
                dodgeDir = moveDir; // 회피 방향 기억
                rigid.AddForce(dodgeDir.normalized * dodgePower, ForceMode.Impulse);
            }

            if(Object.HasInputAuthority)
                RPC_animatonSetTrigger("doDodge");

            isDodge = true;
            StartCoroutine(PlayerDodgeOut(0.5f));
        }
    }


    private void PlayerAttack()
    {
        if(!Object.HasStateAuthority)
            return;
        if (attackInput && !isAttack && !isDodge && !isInteract && !hpHandler.getIsHit())
        {
            Debug.Log("PlayerAttack");
            //attackHandler.StopReload();
            SetIsAttack(true);
            rigid.velocity = new Vector3(0f, rigid.velocity.y, 0f);
            attackHandler.DoAttack(new Vector3(aimForwardVector.x, 0, aimForwardVector.z));
        }
    }
    public void PlayerReload()
    {
        if(!Object.HasStateAuthority)
            return;

        if (!reloadInput || isReload)
            return;
        
        if(isJump || isDodge || isAttack || isInteract || hpHandler.getIsHit())
            return;
        
        Debug.Log("PlayerReload()");
        attackHandler.DoReload();
    }
    private void PlayerInteract()
    {
        if(!Object.HasStateAuthority)
            return;
        if (interactInput && !isAttack && !isDodge && !hpHandler.getIsHit())
        {
            Debug.Log("PlayerInteract");
            attackHandler.StopReload();
            playerActionHandler.Interact();
        }
    }

    public void SetIsAttack(bool bol)
    {
        isAttack = bol;
    }
    public void SetIsReload(bool bol)
    {
        isReload = bol;
    }
    public void SetIsInteract(bool bol)
    {
        isInteract = bol;
    }
    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        isControllerEnable = isEnabled;
    }
    public bool GetCharacterControllerEnabled()
    {
        return isControllerEnable;
    }
    IEnumerator PlayerDodgeOut(float second)
    {
        yield return new WaitForSeconds(second);
        isDodge = false;
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_LookForward(Vector3 moveDir)
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
}
