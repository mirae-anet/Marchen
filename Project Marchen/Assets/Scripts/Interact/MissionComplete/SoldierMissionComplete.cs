using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SoldierMissionComplete : MissionComplete
{
    public override void OnMissionComplete(NetworkObject networkObject)
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
