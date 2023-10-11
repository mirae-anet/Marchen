using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 1 스테이지의 보스인 하트퀸을 처치하면 실행되는 동작들.
public class HeartQueenMissionComplete : MissionComplete
{
    public NetworkBehaviour storyTextPrefab; 
    public NetworkBehaviour potalPrefab; 

    /// @brief 도서관 내부로 돌아가는 포탈 생성, 관련된 스토리 텍스트 출력.
    public override void OnMissionComplete(NetworkObject networkObject)
    {

        if(Runner != null && Object.HasStateAuthority)
        {
            //spawn the text
            Runner.Spawn(storyTextPrefab, networkObject.transform.position - networkObject.transform.forward.normalized * 2, Quaternion.LookRotation(networkObject.transform.forward));
            //spawn the portal
            Runner.Spawn(potalPrefab, networkObject.transform.position + networkObject.transform.forward.normalized * 2, Quaternion.LookRotation(networkObject.transform.forward));

            GameManager.instance.AliceStageClear();
        }
    }
}
