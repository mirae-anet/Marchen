using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HeartQueenHPHandler : SkinnedEnemyHPHandler
{
    const int BossStartHP = 5000;

    protected override void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                HP = BossStartHP;
                isDead = false;
            }
            
            if(heartBar != null)
            {
                heartBar.SetMaxHP(BossStartHP);
                heartBar.SetSlider(HP);
            }
        }
        else
        {
            if(heartBar != null)
            {
                heartBar.SetMaxHP(BossStartHP);
                heartBar.SetSlider(HP);
            }
        }

        isInitialized = true;
    }

    protected override IEnumerator OnDeadCO()
    {
        anim.SetTrigger("doDie");
        if(Object.HasStateAuthority)
        {
            EnemyHPHandler[] enemyHPHandlers = FindObjectsOfType<EnemyHPHandler>();
            if(enemyHPHandlers.Length > 0)
            {
                for(int i = 0; i < enemyHPHandlers.Length; i++)
                {
                    if(enemyHPHandlers[i] != null)
                    {
                        enemyHPHandlers[i].OnTakeDamage("", Object, 255, transform.position); //MAX 즉사
                    }
                }
            }
        }

        yield return new WaitForSeconds(10.0f);

        if(Object.HasStateAuthority)
        {
            if(Spawner != null)
            {
                Spawner.gameObject.SetActive(true);
                Spawner.GetComponent<SpawnHandler>().SetTimer(); //host mirgation 때도 실행됨. 추후에 옳기기.
            }
            Runner.Despawn(Object);
        }
    }
    private void OnDestroy() {
    }
}
