using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCheckAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(other.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
        {
            string nickName = other.transform.root.GetComponent<NetworkPlayer>().nickName.ToString();
            hpHandler.OnTakeDamage(nickName,(byte)255,transform.position);
        }
    }
}
