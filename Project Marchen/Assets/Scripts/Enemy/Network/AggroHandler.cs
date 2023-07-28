using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 일정한 범위 안에 플레이어가 있으면 타겟으로 설정.
/// @see TargetHanlder
public class AggroHandler : NetworkBehaviour
{
    private void OnTriggerEnter(Collider target)
    {
        if(!Runner.IsServer)
            return;

        if (target.tag == "Player")
        {
            Transform player = target.GetComponentInParent<Transform>().root; // Player 최상위 오브젝트 Transform
            gameObject.GetComponentInParent<TargetHandler>().SetTarget(player);
        }
    }
}
