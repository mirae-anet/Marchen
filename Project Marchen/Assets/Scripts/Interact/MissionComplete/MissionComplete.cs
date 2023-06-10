using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionComplete : NetworkBehaviour
{
    public virtual void OnMissionComplete(NetworkObject networkObject){
        Debug.Log("상황에 맞는 스크립트로 변경하세요");
    }
}
