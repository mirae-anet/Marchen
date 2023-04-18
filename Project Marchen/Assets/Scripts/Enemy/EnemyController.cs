using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private EnemyMain enemyMain;

    void Awake()
    {
        enemyMain = GetComponent<EnemyMain>();
    }

    void Update()
    {
        
    }
}
