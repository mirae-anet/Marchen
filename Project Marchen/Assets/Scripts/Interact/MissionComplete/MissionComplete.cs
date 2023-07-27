using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @breif 특정한 미션을 달성한 경우 실행할 동작을 정의하는 스크립트의 부모 클래스.
public class MissionComplete : NetworkBehaviour
{
    /// @brief 상황에 맞는 동작을 처리하도록 구체화하면 됨.
    /// @param networkObject 해당 메서드를 호출하는 게임 오브젝트의 networkObject
    public virtual void OnMissionComplete(NetworkObject networkObject){
        Debug.Log("상황에 맞는 스크립트로 변경하세요");
    }
}
