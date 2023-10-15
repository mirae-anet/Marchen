using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

/// @brief 1 stage의 boss인 HeartQueen의 HP와 관련된 클래스
public class BossHPHandler : SkinnedEnemyHPHandler
{
    int BossStartHP;

    protected override void Start()
    {
        setBossHPbyPlayerCount();

        if (!skipSettingStartValues)
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

    /// @brief 사망 시 동작.
    /// @details 모든 에너미 사망, spawner에게 사망 시 동작을 수행하도록 지시.
    /// @see CountSpawnHandler
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
                Spawner.GetComponent<SpawnHandler>().SetTimer(); //host mirgation 때도 실행?
            }
            Runner.Despawn(Object);
        }
    }

    private void setBossHPbyPlayerCount()
    {
        int playerCount = Runner.ActivePlayers.Count();

        BossStartHP = playerCount * 1000;
    }

    private void OnDestroy() {
    }
}
