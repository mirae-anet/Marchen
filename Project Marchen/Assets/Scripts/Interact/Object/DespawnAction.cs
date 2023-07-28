using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 일정한 범위에 들어서면 기존에 스폰되어있던 모든 에너미를 죽인다.
public class DespawnAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
        if(playerActionHandler != null)
            playerActionHandler.action(transform);
        
        Destroy(transform.gameObject);
    }
}
