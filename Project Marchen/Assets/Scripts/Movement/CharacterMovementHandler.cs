using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0; //카메라 상하

    //other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    Camera localCamera; 

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        localCamera = GetComponentInChildren<Camera>();
    }
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;       
    }
    private void Update()
    {
        //카메라 상하 움직임은 로컬에서 처리
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;   
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90); //위 아래 모가지 한계선
        localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0); //x축 기준으로 카메라 회전?
    }
    public override void FixedUpdateNetwork()
    {
        //get NetworkInputData from Client
        if(GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the view
            networkCharacterControllerPrototypeCustom.Rotate(networkInputData.rotationInput); //Rotate method 구현해야함.

            //move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection);

            //Jump after move
            if(networkInputData.isJumpPressed)
            {
                networkCharacterControllerPrototypeCustom.Jump();
            }

        }
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}
