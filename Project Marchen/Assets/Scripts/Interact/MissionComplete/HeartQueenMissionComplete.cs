using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartQueenMissionComplete : MissionComplete
{
    public NetworkBehaviour storyTextPrefab; 
    public NetworkBehaviour potalPrefab; 
    public override void OnMissionComplete(NetworkObject networkObject)
    {

        if(Runner != null && Object.HasStateAuthority)
        {
            //spawn the portal
            Runner.Spawn(storyTextPrefab, networkObject.transform.position - networkObject.transform.forward.normalized * 2, Quaternion.LookRotation(networkObject.transform.forward));
            //spawn the text
            Runner.Spawn(potalPrefab, networkObject.transform.position + networkObject.transform.forward.normalized * 2, Quaternion.LookRotation(networkObject.transform.forward));
        }
    }
}
