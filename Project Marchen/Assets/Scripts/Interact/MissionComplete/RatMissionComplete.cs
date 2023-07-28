using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 1 스테이지의 농장에서 일정한 수의 쥐들을 처치하면 실행할 동작.
public class RatMissionComplete : MissionComplete
{
    public NetworkBehaviour prefab; 
    /// @brief 배터리 스폰. RatCountSpawner 디스폰.
    public override void OnMissionComplete(NetworkObject networkObject)
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            Runner.Spawn(prefab, networkObject.transform.position, Quaternion.LookRotation(networkObject.transform.forward));
            Runner.Despawn(Object);
        }
    }
}
