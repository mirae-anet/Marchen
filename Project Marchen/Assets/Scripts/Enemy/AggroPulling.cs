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
            
            // 잡몹이면
            if (gameObject.GetComponentInParent<EnemyController>() != null)
            {
                gameObject.GetComponentInParent<EnemyController>().SetTarget(player);
                Debug.Log(gameObject.GetComponentInParent<EnemyMain>().name + " -> " + player);
            }
            else
            {
                gameObject.GetComponentInParent<BossController>().SetTarget(player);
                Debug.Log(gameObject.GetComponentInParent<BossMain>().name + " -> " + player);
            }
            

            gameObject.SetActive(false);
        }
    }
}