using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 플레이어 리스폰 관련 클래스
public class CharacterRespawnHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;

    /// @brief 리스폰 요청의 여부.
    bool isRespawnRequested = false;

    /// @brief 리스폰 지역
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
        Cursor.lockState = CursorLockMode.Locked; // 추후에 옮기기.
        Cursor.visible = false; // 추후에 옮기기.

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

    /// @brief 리스폰을 요청
    /// @see isRespawnRequested, HPHandler.ServerReviveCO(), NetworkPlayer.OnSceneLoaded()
    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    /// @brief 리스폰 실행
    /// @see Utils.GetRandomSpawnPoint(), HpHandler.OnRespawned()
    void Respawn()
    {
        transform.position = Utils.GetRandomSpawnPoint(spawnPoint, 5f);
        hpHandler.OnRespawned();
        isRespawnRequested = false;
    }

    /// @brief 리스폰 위치를 변경.
    /// @see SetSpawnAreaAction
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
