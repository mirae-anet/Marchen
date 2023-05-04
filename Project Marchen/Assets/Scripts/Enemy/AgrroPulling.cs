using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgrroPulling : MonoBehaviour
{
    private void OnTriggerEnter(Collider target)
    {
        if (target.tag == "Player")
        {
            Transform player = target.GetComponent<Transform>();
            gameObject.GetComponentInParent<EnemyController>().SetTarget(player);

            gameObject.SetActive(false);
        }
    }
}