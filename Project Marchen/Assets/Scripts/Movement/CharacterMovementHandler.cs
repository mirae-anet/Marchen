using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;
    bool isControllerEnable = true;

    //other components
    [Header("Rotate")]
    [SerializeField]
    private Transform playerModel;
    // [SerializeField]
    // // private Transform playerBody;
    // NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    NetworkPlayerController networkPlayerController;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        // networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        networkPlayerController = GetComponent<NetworkPlayerController>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }
    void Start()
    {
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
            if(!Object.HasStateAuthority)
                return;

            if(!isControllerEnable)
                return;
            
            // 입력값 저장
            networkPlayerController.SetInput(networkInputData);
    
            // 바닥 체크
            networkPlayerController.GroundCheck();
    
            // 플레이어 조작
            networkPlayerController.PlayerMove();
            networkPlayerController.PlayerJump();
            networkPlayerController.PlayerDodge();
            
            //Check if we've fallen off the world
            CheckFallRespawn();

        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        // networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());
        transform.position = Utils.GetRandomSpawnPoint();
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
        // networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
        isControllerEnable = isEnabled;
    }
}
