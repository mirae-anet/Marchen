using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP {get; set;}

    [Networked(OnChanged = nameof(OnStateChanged))]
    private bool isDead {get; set;}
    bool isDamage = false;
    bool isInitialized = false;
    const byte startingHP = 100;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public GameObject playerModel;
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
    HeartBar heartBar;

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
        heartBar = GetComponentInChildren<HeartBar>();
    }
    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
                HP = startingHP;

            if(heartBar != null)
            {
                heartBar.SetMaxHP(startingHP);
                heartBar.SetSlider(HP);
            }
            isDead = false;
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

    IEnumerator OnHealCO()
    {
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.green;

        yield return new WaitForSeconds(0.6f);

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }

    IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.yellow;

        if(Object != null && Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.5f);

        //화면 정상화
        if(Object != null && Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);

        yield return new WaitForSeconds(0.5f);

        //아바타 정상화
        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }
    IEnumerator OnDeadCO()
    {
        anim.SetTrigger("doDie");

        yield return new WaitForSeconds(2.0f);

        playerModel.gameObject.SetActive(false);
    }

    IEnumerator ServerReviveCO()
    {
        Debug.Log($"{Time.time} ServerRevive");
        yield return new WaitForSeconds(2.5f);
        characterRespawnHandler.RequestRespawn();
    }

    //Function only called on the server
    public void OnTakeDamage(string damagedByNickname, byte damageAmount, Vector3 AttackPostion)
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
    //method overload

    public void OnHeal(byte HealAmount)
    {
        if(!Object.HasStateAuthority)
            return;
        //only take damage while alive
        if(isDead)
            return;

        //Ensure that we cannot flip the byte as it can't handle minus values
        if(HealAmount > startingHP - HP)
            HealAmount = (byte)(startingHP - HP);
        HP += HealAmount;

        RPC_OnHPIncreased();

        Debug.Log($"{Time.time} {transform.name} healed got {HP} left");
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        byte newHP = changed.Behaviour.HP;
        if(changed.Behaviour.heartBar != null)
            changed.Behaviour.heartBar.SetSlider(newHP);
        
        changed.LoadOld();
        byte oldHP = changed.Behaviour.HP;

        if(newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnHPIncreased()
    {
        if(!isInitialized)
            return;
        StartCoroutine(OnHealCO());
    }

    private void OnHPReduced()
    {
        if(!isInitialized)
            return;
        
        StartCoroutine(OnHitCO());
    }
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

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");
        StartCoroutine(OnDeadCO());

        if(Object.HasStateAuthority)
        {
            gameObject.tag = "Respawn"; // Player 태그 갖고 있으면 Enemy 타겟팅 망가짐
            rigid.velocity = Vector3.zero;
            rigid.isKinematic = true;
            rigid.detectCollisions = false;
            hitboxRoot.HitboxRootActive = false; //죽고나서는 데미지 안 들어감
            networkPlayerController.SetCharacterControllerEnabled(false);
            StartCoroutine(ServerReviveCO());
        }
    }

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

    public void OnRespawned()
    {
        HP = startingHP;
        isDead = false;
    }

    public bool getIsHit()
    {
        return isDamage;
    }

    public void KnockBack(Vector3 AttackPostion)
    {
        Vector3 reactDir = (transform.position - AttackPostion).normalized;

        rigid.AddForce(Vector3.up * 25f, ForceMode.Impulse);
        rigid.AddForce(reactDir * 10f, ForceMode.Impulse);
    }
    public bool GetIsDead()
    {
        return isDead;
    }
}
