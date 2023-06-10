using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyHPHandler : NetworkBehaviour
{
    public bool skipSettingStartValues = false;
    bool isInitialized = false;
    [Range(1f, 1000f)]
    const int startingHP = 100;

    protected bool isDamage = false;

    [Networked(OnChanged = nameof(OnStateChanged))]
    private bool isDead {get; set;}

    [Networked(OnChanged = nameof(OnHPChanged))]
    int HP {get; set;}

    [Range(0f, 5f)]
    public float knockbackForce = 0.3f;

    // other component
    public NetworkObject Spawner;
    private MeshRenderer[] meshs;
    protected Animator anim;
    HitboxRoot hitboxRoot;
    TargetHandler targetHandler;
    EnemyAttackHandler enemyAttackHandler; 
    HeartBar heartBar;
    public AudioSource deathSource;

    protected virtual void Awake()
    {
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>(); 
        targetHandler = GetComponent<TargetHandler>();
        enemyAttackHandler = GetBehaviour<EnemyAttackHandler>();
        heartBar = GetComponentInChildren<HeartBar>();
    }

    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                HP = startingHP;
                isDead = false;
            }
            
            if(heartBar != null)
            {
                heartBar.SetMaxHP(startingHP);
                heartBar.SetSlider(HP);
            }
        }
        else
        {
            if(heartBar != null)
            {
                heartBar.SetMaxHP(startingHP);
                heartBar.SetSlider(HP);
            }
        }

        isInitialized = true;
    }

    protected virtual IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;
        deathSource.Play();
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
            if(Spawner != null)
            {
                Spawner.gameObject.SetActive(true);
                Spawner.GetComponent<SpawnHandler>().SetTimer(); //host mirgation 때도 실행됨. 추후에 옳기기.
            }
            Runner.Despawn(Object);
        }
    }

    public void OnTakeDamage(string damagedByNickname, NetworkObject damagedByNetworkObject, int damageAmount, Vector3 AttackPostion)
    {
        //only take damage while alive
        if(isDead)
            return;
        if(isDamage)
            return;

        if(damageAmount > HP)
            damageAmount = HP;
        HP -= damageAmount;

        Debug.Log($"{Time.time} Enemy took damage got {HP} left");

        if(HP <= 0)
        {
            Debug.Log($"{Time.time} {transform.name} died");
            isDead = true;
        }
        else
        {
            KnockBack(AttackPostion);
            enemyAttackHandler.AttackCancel();
            targetHandler.SetTarget(damagedByNetworkObject.transform); // 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
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

        int newHP = changed.Behaviour.HP;

        if(changed.Behaviour.heartBar != null)
            changed.Behaviour.heartBar.SetSlider(newHP);
        
        changed.LoadOld();
        int oldHP = changed.Behaviour.HP;

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
