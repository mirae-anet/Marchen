using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BulletAtack : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            if(other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }

            HPHandler hpHandler = other.transform.root.GetComponent<HPHandler>();
            int damageAmount = 50;
            if (hpHandler != null)
            {
                Destroy(gameObject);
                hpHandler.OnTakeDamage(other.transform.name, damageAmount, other.transform.position);
            }
        }
    }
}
