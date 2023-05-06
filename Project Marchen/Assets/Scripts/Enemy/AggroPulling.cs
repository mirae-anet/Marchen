using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroPulling : MonoBehaviour
{
    private void OnTriggerEnter(Collider target)
    {
        if (target.tag == "Player")
        {
            Transform player = target.GetComponentInParent<Transform>().root; // Player 최상위 오브젝트 Transform
            gameObject.GetComponentInParent<EnemyController>().SetTarget(player);
            Debug.Log(player);
            gameObject.SetActive(false);
        }
    }
}