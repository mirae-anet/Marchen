using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeAttack : NetworkBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            HPHandler hpHandler = other.transform.root.GetComponent<HPHandler>();
            int damageAmount = 30;
            if (hpHandler != null)
            {
                hpHandler.OnTakeDamage(other.transform.name, damageAmount, other.transform.position);
            }
        }
    } 
}

