using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RatMissionComplete : MissionComplete
{
    public NetworkBehaviour prefab; 
    public override void OnMissionComplete(NetworkObject networkObject)
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            Runner.Spawn(prefab, networkObject.transform.position, Quaternion.LookRotation(networkObject.transform.forward));
            Runner.Despawn(Object);
        }
    }
}
