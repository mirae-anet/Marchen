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
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        // networkyerController = GetComponent<NetworkPlayerController>();
        hpHandler = GetComponent<HPHandler>();
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
            if(isRespawnRequested)
                Respawn();
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        transform.position = Utils.GetRandomSpawnPoint(spawnPoint, 5f);
        hpHandler.OnRespawned();
        isRespawnRequested = false;
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
