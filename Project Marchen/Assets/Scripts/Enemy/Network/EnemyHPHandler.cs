using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyHPHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;
    bool isInitialized = false;
    bool isDamage = false;

    [Range(1f, 1000f)]
    const byte startingHP = 100;

    // [Networked(OnChanged = nameof(OnStateChanged))]
    private bool isDead {get; set;}

    // [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP {get; set;}

    [Range(0f, 5f)]
    public float knockbackForce = 0.3f;

    // other component
    public NetworkObject enemySpawner;
    private MeshRenderer[] meshs;
    private Animator anim;
    NetworkInGameMessages networkInGameMessages;

    void Awake()
    {
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    void Start()
    {
        if(!skipSettingStartValues){
            HP = startingHP;
            isDead = false;
        }

        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        //아바타 정상화
        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }

    public void OnTakeDamage(string damageCausedByPlayerNickname, byte damageAmount, Vector3 AttackPostion)
    {
        //only take damage while alive
        if(isDead)
            return;
        if(isDamage)
            return;

        //Ensure that we cannot flip the byte as it can't handle minus values.
        if(damageAmount > HP)
            damageAmount = HP;
        HP -= damageAmount;

        Debug.Log($"{Time.time} Enemy took damage got {HP} left");

        //player died
        if(HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickname, $"Killed Enemy");
            Debug.Log($"{Time.time} {transform.name} died");
            isDead = true;
        }
        else
        {
            KnockBack(AttackPostion);
        }
    }
    private void OnDestroy() 
    {
        if(enemySpawner != null)
            enemySpawner.GetComponent<EnemySpawnHandler>().SetTimer(); //host mirgation 때도 실행됨. 추후에 옳기기.
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;
        reactDir.y = 0f;

        transform.position += reactDir * knockbackForce;
    }
}
