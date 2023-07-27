using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

/// @breif 플레이어 HP 관련 클래스
public class HPHandler : NetworkBehaviour
{
    /// @breif 현재 HP를 나타내는 변수
    /// @details 동기화 되어있음. 값은 서버만 변경할 수 있다. 값의 변화가 생기면 모두 OnHPChanged라는 콜백 함수를 호출한다.
    [Networked(OnChanged = nameof(OnHPChanged))]
    int HP {get; set;}

    /// @breif 현재 죽었는지 살았는지 나타내는 변수
    /// @details 동기화 되어있음. 값은 서버만 변경할 수 있다. 값의 변화가 생기면 모두 OnStateChanged라는 콜백 함수를 호출한다.
    [Networked(OnChanged = nameof(OnStateChanged))]
    private bool isDead {get; set;}
    /// @breif 현재 데미지를 받았는지 나타내는 변수
    bool isDamage = false;
    bool isInitialized = false;
    /// @breif 시작 HP
    const int startingHP = 100;

    /// @breif 피격 시 화면 효과 (색상)
    public Color uiOnHitColor;
    /// @breif 피격 시 화면 효과
    public Image uiOnHitImage;

    public GameObject playerModel;
    /// @breif 호스트마이그레션 시 초기화하지 않도록 하기 위함.
    public bool skipSettingStartValues = false; //Use when HostMirgration copy HP

    //other components
    private MeshRenderer[] meshs;
    HitboxRoot hitboxRoot;
    CharacterRespawnHandler characterRespawnHandler;
    NetworkPlayerController networkPlayerController;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    Animator anim;
    Rigidbody rigid;
    public HeartBar heartBar;
    public HeartBar myHeartBar;
    public AudioSource deadSound;
    public AudioSource hitSound;
    public AudioSource healSound;

    private void Awake()
    {
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        characterRespawnHandler = GetComponent<CharacterRespawnHandler>();
        networkPlayerController = GetComponent<NetworkPlayerController>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>(); 
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
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
        }

        if(heartBar != null)
        {
            heartBar.SetMaxHP(startingHP);
            heartBar.SetSlider(HP);
            myHeartBar.SetMaxHP(startingHP);
            myHeartBar.SetSlider(HP);
        }

        isInitialized = true;
    }

    /// @breif 회복 시 동작 
    /// @details 모든 컴퓨터에서 동작함. mesh의 색상 변화.
    IEnumerator OnHealCO()
    {
        healSound.Play();
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.green;

        yield return new WaitForSeconds(0.6f);

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }

    /// @breif 피격시 동작 
    /// @details 모든 컴퓨터에서 동작함. mesh, 플레이어 화면의 색상 변화, isDamage 변수값 변경, 효과음.
    IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;
        hitSound.Play();

        if(Object != null && Object.HasInputAuthority)
            hitboxRoot.HitboxRootActive = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.yellow;

        if(Object != null && Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(1f);

        isDamage = false;

        //화면 정상화
        if(Object != null && Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);

        yield return new WaitForSeconds(0.5f);

        //아바타 정상화

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;

        if(Object != null && Object.HasInputAuthority)
            hitboxRoot.HitboxRootActive = true;
    }
    /// @breif 사망시 동작 
    /// @details 모든 컴퓨터에서 동작함. 아바타 비활성화, 애니매이션, 효과음.
    IEnumerator OnDeadCO()
    {
        anim.SetTrigger("doDie");
        deadSound.Play();

        yield return new WaitForSeconds(2.0f);

        playerModel.gameObject.SetActive(false);
    }

    /// @breif 사망으로부터 일정시간이 지난 후 리스폰 요청함.
    /// @details 서버에서만 실행함.
    IEnumerator ServerReviveCO()
    {
        Debug.Log($"{Time.time} ServerRevive");
        yield return new WaitForSeconds(2.5f);
        characterRespawnHandler.RequestRespawn();
    }

    /// @breif 해당 플레이어의 HP를 감소시키는 메서드.
    /// @details HP를 감소, 죽었는지 확인, KnockBack 호출.
    /// @param damagedByNickname 공격한 오브젝트의 닉네임.
    /// @param damageAmount 받은 피해량
    /// @param AttackPosition 공격 위치. KnockBack 방향 계산을 위해서 필요함.
    public void OnTakeDamage(string damagedByNickname, int damageAmount, Vector3 AttackPostion)
    {
        if(!Object.HasStateAuthority)
            return;
        //only take damage while alive
        if(isDead)
            return;
        if(isDamage)
            return;

        if(damageAmount > HP)
            damageAmount = HP;
        HP -= damageAmount;

        Debug.Log($"{Time.time} {transform.name} took damage got {HP} left");

        //player died
        if(HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damagedByNickname, $"Killed <b>{networkPlayer.nickName.ToString()}</b>");
            Debug.Log($"{Time.time} {transform.name} died");
            isDead = true;
        }
        else
        {
            KnockBack(AttackPostion);
        }
    }

    /// @breif 회복 시 동작 
    /// @details 서버에서만 동작함. 플레이어의 HP 상승.
    public void OnHeal(int HealAmount)
    {
        if(!Object.HasStateAuthority)
            return;
        //only take damage while alive
        if(isDead)
            return;

        if(HealAmount > startingHP - HP)
            HealAmount = (int)(startingHP - HP);
        HP += HealAmount;

        RPC_OnHPIncreased();

        Debug.Log($"{Time.time} {transform.name} healed got {HP} left");
    }

    /// @breif HP의 값이 변화할 시 호출되는 콜백함수.
    /// @details 모든 컴퓨터에서 동작.
    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        int newHP = changed.Behaviour.HP;
        if(changed.Behaviour.heartBar != null)
            changed.Behaviour.heartBar.SetSlider(newHP);
        if(changed.Behaviour.myHeartBar != null)
            changed.Behaviour.myHeartBar.SetSlider(newHP);
        
        changed.LoadOld();
        int oldHP = changed.Behaviour.HP;

        if(newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    /// @breif 회복 시 OnHeal 메서드에서 호출. 
    /// @details 서버에서 모든 컴퓨터에게 OnHealCO를 실행하도록 함.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnHPIncreased()
    {
        if(!isInitialized)
            return;
        StartCoroutine(OnHealCO());
    }

    
    /// @breif HP 감소 시 처리. OnHitCO()호출.
    /// @see OnHitCO()
    private void OnHPReduced()
    {
        if(!isInitialized)
            return;
        
        StartCoroutine(OnHitCO());
    }

    /// @breif isDead의 값이 변화할 시 호출되는 콜백함수.
    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");

        bool isDeadCurrent = changed.Behaviour.isDead;

        changed.LoadOld();
        bool isDeadOld = changed.Behaviour.isDead;

        if(isDeadCurrent)
            changed.Behaviour.OnDeath();
        else if(!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }

    /// @breif 사망 시 처리. OnDeadCO() 실행.
    /// @details rigid body가 움직이지 않도록 처리. hitbox를 비활성화. 아바타를 조종할 수 없도록 처리. ServerReviveCO 실행.
    /// @see OnDeadCO(), ServerReviveCO()
    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");
        StartCoroutine(OnDeadCO());

        if(Object.HasStateAuthority)
        {
            rigid.velocity = Vector3.zero;
            rigid.isKinematic = true;
            rigid.detectCollisions = false;
            hitboxRoot.HitboxRootActive = false; //죽고나서는 데미지 안 들어감
            networkPlayerController.SetCharacterControllerEnabled(false);
            StartCoroutine(ServerReviveCO());
        }
    }

    /// @breif 부활 시 처리.
    /// @details 모든 컴퓨터에서 실행
    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");
        // move to respawn place
        if(Object.HasInputAuthority)
            uiOnHitImage.color = new Color(0,0,0,0);

        rigid.isKinematic = false;
        rigid.detectCollisions = true;
        hitboxRoot.HitboxRootActive = true;
        playerModel.gameObject.SetActive(true);
        networkPlayerController.SetCharacterControllerEnabled(true);
    }

    /// @breif 부활 시 처리.
    /// @see CharacterRespawnHandler.Respawn()
    public void OnRespawned()
    {
        HP = startingHP;
        isDead = false;
    }

    /// @breif isDamage 값에 리턴
    /// @return bool isDamage
    public bool getIsHit()
    {
        return isDamage;
    }

    /// @breif 넉백 효과.
    /// @param AttackPosition 공격 받은 방향
    public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;

        rigid.AddForce(Vector3.up * 25f, ForceMode.Impulse);
        rigid.AddForce(reactDir * 10f, ForceMode.Impulse);
    }

    /// @breif isDead 값에 리턴
    /// @return bool isDead
    public bool GetIsDead()
    {
        return isDead;
    }
}
