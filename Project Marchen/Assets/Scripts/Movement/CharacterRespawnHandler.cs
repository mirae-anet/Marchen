using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// public class CharacterMovementHandler : NetworkBehaviour
public class CharacterRespawnHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;

    //other components
    [Header("Rotate")]
    [SerializeField]
    // NetworkPlayerController networkPlayerController;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        // networkyerController = GetComponent<NetworkPlayerController>();
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
            if(hpHandler.GetIsDead())
                return;
        }

        //get NetworkInputData from Client
        if(GetInput(out NetworkInputData networkInputData))
        {
            if(!Object.HasStateAuthority)
                return;

            CheckFallRespawn();

        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
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

                hpHandler.OnTakeDamage(networkPlayer.nickName.ToString(),(byte)255,transform.position);
            }
        }
    }
}
