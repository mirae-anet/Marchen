using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    CharacterMovementHandler characterMovementHandler;
    private void Awake()
    {
        // 호스트에게 공유되지 않는 클라이언트의 view 움직임
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
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
        // 호스트에게 공유되지 않는 클라이언트의 view 움직임
        characterMovementHandler.SetViewInputVector(viewInputVector);

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump
        isJumpButtonPressed = Input.GetButtonDown("Jump");

    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //view data
        networkInputData.rotationInput = viewInputVector.x; //only x
        //move data
        networkInputData.movementInput = moveInputVector;
        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        return networkInputData;
    }
}
