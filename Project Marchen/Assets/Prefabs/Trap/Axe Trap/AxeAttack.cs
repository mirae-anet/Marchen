using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeAttack : NetworkBehaviour
{

    /*public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;

        rigid.AddForce(Vector3.up * 25f, ForceMode.Impulse);
        rigid.AddForce(reactDir * 10f, ForceMode.Impulse);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            HPHandler hpHandler = other.transform.root.GetComponent<HPHandler>();
            int damageAmount = 50;
            if (hpHandler != null)
            {
                hpHandler.OnTakeDamage(other.transform.name, damageAmount, other.transform.position);
               
            }
        }
    } 
}

