using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @breif 플레이어의 마우스, 키보드 입력을 받아서 전달.
public class CharacterInputHandler : MonoBehaviour
{
    /// @breif 키보드 상하좌우 입력
    Vector2 moveInputVector = Vector2.zero;
    /// @breif 마우스 상하좌우 입력
    Vector2 viewInputVector = Vector2.zero;
    /// @breif 움직임과 관련된 입력이 있는지.
    bool isMove = false;
    bool walkInput = false;
    bool jumpInput = false;
    bool dodgeInput = false;
    bool attackInput = false;
    bool reloadInput = false;
    bool interactInput = false;
    // 설명 추가 부탁
    bool escEnable = true; 

    //other components
    LocalCameraHandler localCameraHandler;
    NetworkPlayerController networkPlayerController;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        networkPlayerController = GetComponent<NetworkPlayerController>();
    }
    void Start()
    {
        
    }
    /// @breif 각각의 플레이어가 속한 컴퓨터에서 입력을 받는다.
    void Update()
    {
        //호스트에서는 실행x
        //일반적으로 플레이어가 직접 조작하는 캐릭터의 입력을 처리하는 경우 클라이언트가 입력 권한을 가지게 됩니다.
        if(!networkPlayerController.Object.HasInputAuthority){return;} 

        //view input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y"); //new
        // viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // 이동 입력 벡터
        isMove = (moveInput.magnitude != 0); // moveInput의 길이로 입력 판정

        if (Input.GetButtonDown("Jump"))
            jumpInput = true;
        if(Input.GetButtonDown("Walk"))
            walkInput = true;
        if(Input.GetButtonDown("Dodge"))
            dodgeInput = true;
        if(Input.GetButtonDown("Reload"))
            reloadInput = true;
        if (escEnable == true)
        {
            if (Input.GetButtonDown("Fire1"))
                attackInput = true;
        }
        if(Input.GetButtonDown("Interact"))
            interactInput = true;

        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector);

    }
    /// @breif 각각의 플레이어가 속한 컴퓨터에서 입력값을 서버로 보낸다.
    /// @details NetworkInputData의 데이터 구조체로 보낸다. 보낸 후 입력값을 초기화 한다. localCamraHandler에서 이동방향과 3차원 조준 방향을 받아온다.
    /// @see NetworkInputData
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //move data
        networkInputData.isMove = isMove;
        //Jump data
        networkInputData.jumpInput = jumpInput;
        //walk data
        networkInputData.walkInput = walkInput;
        //dodge data
        networkInputData.dodgeInput = dodgeInput;
        //attack data
        networkInputData.attackInput = attackInput;
        //reload data
        networkInputData.reloadInput = reloadInput;
        //interact data
        networkInputData.interactInput = interactInput;
        //moveDir
        networkInputData.moveDir = localCameraHandler.getMoveDir(moveInputVector);
        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.getAimForwardVector();

        //Reset variables now that we have read their status
        // isMove = false;
        jumpInput = false;
        walkInput = false;
        dodgeInput = false;
        attackInput = false;
        reloadInput = false;
        interactInput = false;

        return networkInputData;
    }

    /// @breif 설명 추가 부탁
    public void EnableinPut(bool enable)
    {
        escEnable = enable;
    }

}
