using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
        if(playerActionHandler != null)
            playerActionHandler.action(transform);
        
        Destroy(transform.root.gameObject);
    }
}
