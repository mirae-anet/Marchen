using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    LocalCameraHandler localCameraHandler;
    private void Awake()
    {
        // 호스트에게 공유되지 않는 클라이언트의 view 움직임
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
    }
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //view input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump
        if(Input.GetButtonDown("Jump"))
        {
            isJumpButtonPressed = true;
        }

        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector); //수정? localCameraHandler로 변경?
    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //move data
        networkInputData.movementInput = moveInputVector;
        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        //Reset variables now that we have read their status
        isJumpButtonPressed = false;

        return networkInputData;
    }
}
