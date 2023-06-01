using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// public class CharacterMovementHandler : NetworkBehaviour
public class CharacterRespawnHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;

    bool isRespawnRequested = false;

    [Networked]
    private Vector3 spawnPoint{get; set;}

    //other components
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

        if(Object.HasStateAuthority)
            if(!skipSettingStartValues)
                spawnPoint = Utils.GetRandomSpawnPoint();
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
        transform.position = Utils.GetRandomSpawnPoint(spawnPoint);
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

    public void ChangeSpawnPoint(Vector3 newSpawnPoint)
    {
        if(!Object.HasStateAuthority)
            return;

        CharacterRespawnHandler[] respawnHandlers = FindObjectsOfType<CharacterRespawnHandler>(true);
        if(respawnHandlers.Length > 0)
        {
            for(int i = 0; i < respawnHandlers.Length; i++)
            {
                CharacterRespawnHandler respawnHandler = respawnHandlers[i];
                if(respawnHandler != null)
                {
                    respawnHandler.spawnPoint = newSpawnPoint;
                }
            }
        }

    }
}
