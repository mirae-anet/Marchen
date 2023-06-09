using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isMove = false;
    bool walkInput = false;
    bool jumpInput = false;
    bool dodgeInput = false;
    bool attackInput = false;
    bool reloadInput = false;
    bool interactInput = false;

    //esc
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
    // Update is called once per frame
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

    public void EnableinPut(bool enable)
    {
        escEnable = enable;
    }

}
