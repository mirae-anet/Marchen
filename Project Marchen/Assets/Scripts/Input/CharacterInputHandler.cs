using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrenadeFireButtonPressed = false;
    bool isRocketLauncherFireButtonPressed = false;

    //other components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;
    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //호스트에서는 실행x
        //일반적으로 플레이어가 직접 조작하는 캐릭터의 입력을 처리하는 경우 클라이언트가 입력 권한을 가지게 됩니다.
        if(!characterMovementHandler.Object.HasInputAuthority){return;} 

        //view input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump
        if(Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;
        //fire
        if(Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;

        //Grenade fire
        if(Input.GetKeyDown(KeyCode.G))
            isGrenadeFireButtonPressed = true;

        //Rocket fire
        if(Input.GetButtonDown("Fire2"))
            isRocketLauncherFireButtonPressed = true;

        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector); 
    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //look data
        networkInputData.lookForwardVector = localCameraHandler.transform.forward;
        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.getAimForwardVector();
        //move data
        networkInputData.movementInput = moveInputVector;
        //Jump data
        networkInputData.isJumpButtonPressed = isJumpButtonPressed;
        //Fire data
        networkInputData.isFireButtonPressed = isFireButtonPressed;
        //Grenade data
        networkInputData.isGrenadeFireButtonPressed = isGrenadeFireButtonPressed;
        //Rocket data
        networkInputData.isRocketLauncherFireButtonPressed = isRocketLauncherFireButtonPressed;

        //Reset variables now that we have read their status
        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrenadeFireButtonPressed = false;
        isRocketLauncherFireButtonPressed = false;

        return networkInputData;
    }
}
