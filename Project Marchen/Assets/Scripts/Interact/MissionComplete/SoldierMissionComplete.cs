using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 1 스테이지의 크로켓 경기장의 병사를 일정 수 처치하면 실행할 동작.
public class SoldierMissionComplete : MissionComplete
{
    /// @brief SoilderCountSpawner 디스폰.
    public override void OnMissionComplete(NetworkObject networkObject)
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
