using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyHPHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;
    bool isInitialized = false;
    [Range(1f, 1000f)]
    const byte startingHP = 100;

    bool isDamage = false;

    [Networked(OnChanged = nameof(OnStateChanged))]
    private bool isDead {get; set;}

    [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP {get; set;}

    [Range(0f, 5f)]
    public float knockbackForce = 0.3f;

    // other component
    public NetworkObject enemySpawner;
    private MeshRenderer[] meshs;
    private Animator anim;
    NetworkInGameMessages networkInGameMessages;
    HitboxRoot hitboxRoot;

    void Awake()
    {
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>(); 
    }

    void Start()
    {
        if(!skipSettingStartValues){
            HP = startingHP;
            isDead = false;
        }

        isInitialized = true;
    }

    IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;
        anim.SetBool("isWalk", false);

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        //아바타 정상화
        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }
    IEnumerator OnDeadCO()
    {
        anim.SetTrigger("doDie");

        yield return new WaitForSeconds(2.0f);

        if(Object.HasStateAuthority)
        {
            if(enemySpawner != null)
            {
                enemySpawner.gameObject.SetActive(true);
                enemySpawner.GetComponent<EnemySpawnHandler>().SetTimer(); //host mirgation 때도 실행됨. 추후에 옳기기.
            }
            Runner.Despawn(Object);
        }
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

        if(HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickname, $"Killed Enemy");
            Debug.Log($"{Time.time} {transform.name} died");
            isDead = true;
        }
        else
        {
            KnockBack(AttackPostion);
            // enemyController.SetTarget(other.GetComponentInParent<Transform>().root); // 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
        }
    }
    static void OnStateChanged(Changed<EnemyHPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");

        bool isDeadCurrent = changed.Behaviour.isDead;

        changed.LoadOld();
        bool isDeadOld = changed.Behaviour.isDead;

        if(isDeadCurrent)
            changed.Behaviour.OnDeath();
    }
    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");
        hitboxRoot.HitboxRootActive = false;
        StartCoroutine(OnDeadCO());
    }

    static void OnHPChanged(Changed<EnemyHPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        byte newHP = changed.Behaviour.HP;
        
        changed.LoadOld();
        byte oldHP = changed.Behaviour.HP;

        if(newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    private void OnHPReduced()
    {
        if(!isInitialized)
            return;
        StartCoroutine(OnHitCO());
    }

   private void OnDestroy() 
    {
    }

    public bool GetIsDead()
    {
        return isDead;
    }
    public bool GetIsDamage()
    {
        return isDamage;
    }

    public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;
        reactDir.y = 0f;

        transform.position += reactDir * knockbackForce;
    }

    //animation
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_animatonSetBool(string action, bool isDone)
    // {
    //     anim.SetBool(action, isDone);
    //     // anim.SetTrigger("doJump");
    // }

    // [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_animatonSetTrigger(string action)
    // {
    //     anim.SetTrigger(action);
    // }
}
