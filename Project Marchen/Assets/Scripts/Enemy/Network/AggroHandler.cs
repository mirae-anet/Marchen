using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AggroHandler : NetworkBehaviour
{
    private void OnTriggerEnter(Collider target)
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(!networkRunner.IsServer)
            return;

        if (target.tag == "Player")
        {
            Transform player = target.GetComponentInParent<Transform>().root; // Player 최상위 오브젝트 Transform
            gameObject.GetComponentInParent<TargetHandler>().SetTarget(player);
            gameObject.SetActive(false);
        }
    }
}
