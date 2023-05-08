using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyHPHandler : NetworkBehaviour
{
    public NetworkObject enemySpawner;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy() 
    {
        enemySpawner.GetComponent<EnemySpawnHandler>().SetTimer();    
    }
}
