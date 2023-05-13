using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyHPHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;

    // [Networked(OnChanged = nameof(OnHPChanged))]
    // byte HP {get; set;}
    //

    public NetworkObject enemySpawner;
    void Start()
    {
        if(!skipSettingStartValues){
            // HP = startingHP;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy() 
    {
        if(enemySpawner != null)
            enemySpawner.GetComponent<EnemySpawnHandler>().SetTimer(); //host mirgation 때도 실행됨. 추후에 옳기기.
    }
}