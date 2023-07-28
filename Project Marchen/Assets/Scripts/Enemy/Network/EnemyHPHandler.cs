using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 에너미 HP 관련 클래스.
public class EnemyHPHandler : NetworkBehaviour
{
    /// @brief 호스트마이그레션 시 초기화하지 않도록 하기 위함.
    public bool skipSettingStartValues = false;
    protected bool isInitialized = false;

    [Range(1f, 1000f)]
    /// @brief 시작 HP
    const int startingHP = 100;

    /// @brief 현재 데미지를 받았는지 나타내는 변수
    protected bool isDamage = false;

    /// @brief 현재 죽었는지 살았는지 나타내는 변수
    /// @details 동기화 되어있음. 값은 서버만 변경할 수 있다. 값의 변화가 생기면 모두 OnStateChanged라는 콜백 함수를 호출한다.
    [Networked(OnChanged = nameof(OnStateChanged))]
    protected bool isDead {get; set;}

    /// @brief 현재 HP를 나타내는 변수
    /// @details 동기화 되어있음. 값은 서버만 변경할 수 있다. 값의 변화가 생기면 모두 OnHPChanged라는 콜백 함수를 호출한다.
    [Networked(OnChanged = nameof(OnHPChanged))]
    protected int HP {get; set;}

    [Range(0f, 5f)]
    /// @brief 데미지 받으면 밀려나는 정도 
    public float knockbackForce = 0.3f;

    // other component
    public NetworkObject Spawner;
    private MeshRenderer[] meshs;
    protected Animator anim;
    protected HitboxRoot hitboxRoot;
    TargetHandler targetHandler;
    EnemyAttackHandler enemyAttackHandler; 
    protected HeartBar heartBar;
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

    protected virtual void Start()
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

    /// @brief 피격시 동작 
    /// @details 모든 컴퓨터에서 동작함. mesh의 색상 변화, isDamage 변수값 변경.
    protected virtual IEnumerator OnHitCO()
    {
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

    /// @brief 사망시 동작 
    /// @details 모든 컴퓨터에서 동작함. despawn 및 spawner의 타이머를 재설정.
    protected virtual IEnumerator OnDeadCO()
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

    /// @brief 해당 에너미를 HP를 감소시키는 메서드.
    /// @details HP를 감소, 죽었는지 확인, KnockBack 호출, 공격 캔슬, 타겟 변경.
    /// @param damagedByNickname 공격한 플레이어의 닉네임.
    /// @param damagedByNetworkObject 공격한 플레이어의 NetworkObject.
    /// @param damageAmount 받은 피해량
    /// @param AttackPosition 공격 위치. KnockBack 방향 계산을 위해서 필요함.
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

    /// @brief isDead의 값이 변화할 시 호출되는 콜백함수.
    static void OnStateChanged(Changed<EnemyHPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");

        bool isDeadCurrent = changed.Behaviour.isDead;

        changed.LoadOld();
        bool isDeadOld = changed.Behaviour.isDead;

        if(isDeadCurrent)
            changed.Behaviour.OnDeath();
    }

    /// @brief 사망 시 처리. OnDeadCO() 실행.
    /// @see OnDeadCO()
    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");
        hitboxRoot.HitboxRootActive = false;
        StartCoroutine(OnDeadCO());
    }

    /// @brief HP의 값이 변화할 시 호출되는 콜백함수.
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

    /// @brief HP 감소 시 처리. OnHitCO()호출.
    /// @see OnHitCO()
    private void OnHPReduced()
    {
        if(!isInitialized)
            return;
        StartCoroutine(OnHitCO());
    }

   private void OnDestroy() 
    {
    }

    /// @brief isDead 값에 리턴
    /// @return bool isDead
    public bool GetIsDead()
    {
        return isDead;
    }

    /// @brief isDamage 값에 리턴
    /// @return bool isDamage
    public bool GetIsDamage()
    {
        return isDamage;
    }

    /// @brief 넉백 효과.
    /// @param AttackPosition 공격 받은 방향
    public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;
        reactDir.y = 0f;

        transform.position += reactDir * knockbackForce;
    }

}
