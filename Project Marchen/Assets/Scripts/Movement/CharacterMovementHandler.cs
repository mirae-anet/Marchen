using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;
    private bool isMove;

    //other components
    [Header("Rotate")]
    [SerializeField]
    private Transform playerModel;
    // [SerializeField]
    // private Transform playerBody;
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }
    void Start()
    {
        isMove = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;       
    }
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            if(isRespawnRequested)
            {
                Respawn();
                return;
            }               

            //Don't update the client positon when they are dead
            if(hpHandler.isDead)
                return;
        }

        //get NetworkInputData from Client
        if(GetInput(out NetworkInputData networkInputData))
        {

            // if (isDodge && !playerMain.getIsHit()) // 회피, 피격 중 이동 제한
            //     return;

            //new
            if(networkInputData.isMove)
            {
                Debug.Log($"moveDir is {networkInputData.moveDir}");
                playerModel.forward = networkInputData.moveDir;
                // playerBody.forward = networkInputData.moveDir;
                //rigidBody 필요
                networkCharacterControllerPrototypeCustom.Move(networkInputData.moveDir);
            }

            // if(networkInputData.isFireButtonPressed || networkInputData.isGrenadeFireButtonPressed || networkInputData.isRocketLauncherFireButtonPressed)
            // {
            //     //공격 시 playerBody.forward 방향 전환 필요.
            // }

            //Jump after move
            if(networkInputData.isJumpButtonPressed)
            {
                networkCharacterControllerPrototypeCustom.Jump();
            }

            //Check if we've fallen off the world
            CheckFallRespawn();

            //old
            /**
            //Rotate the transform according to the client aim vector
            transform.forward = networkInputData.lookForwardVector;
            //Cancel out rotation on X axis as we don't want our character to tilt
            //마우스 상하 움직임은 무시
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            // //move
            // Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            // moveDirection.Normalize(); //경록이 moveDir과 일치

            networkCharacterControllerPrototypeCustom.Move(moveDirection);

            //Jump after move
            if(networkInputData.isJumpButtonPressed)
            {
                networkCharacterControllerPrototypeCustom.Jump();
            }

            //Check if we've fallen off the world
            CheckFallRespawn();
            **/
        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());
        hpHandler.OnRespawned();
        isRespawnRequested = false;
    }

    void CheckFallRespawn()
    {
        if(transform.position.y < -12)
        {
            if(Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName.ToString(), "fell off the world");

                Respawn();
            }
        }
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
}
