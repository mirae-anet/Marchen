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
    public bool isDead {get; set;}

    bool isInitialized = false;
    const byte startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;
    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    public GameObject playerModel;
    public GameObject deathGameObjectPrefab;

    //other components
    HitboxRoot hitboxRoot;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>(); 
    }
    void Start()
    {
        HP = startingHP;
        isDead = false;

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        isInitialized = true;

    }

    IEnumerator OnHitCO()
    {
        // 피격시 효과
        bodyMeshRenderer.material.color = Color.red;
        if(Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        //정상화
        bodyMeshRenderer.material.color = defaultMeshBodyColor;
        if(Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator ServerReviveCO()
    {
        Debug.Log($"{Time.time} ServerRevive");
        yield return new WaitForSeconds(2.0f);
        characterMovementHandler.RequestRespawn();
    }

    //Function only called on the server
    public void OnTakeDamage()
    {
        //only take damage while alive
        if(isDead)
            return;
        HP -= 1;

        Debug.Log($"{Time.time} {transform.name} took damage got {HP} left");

        //player died
        if(HP <= 0)
        {
            Debug.Log($"{Time.time} {transform.name} died");
            isDead = true;
            StartCoroutine(ServerReviveCO());
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
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
        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false; //죽고나서는 데미지 안 들어감
        characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");
        // move to respawn place
        if(Object.HasInputAuthority)
            uiOnHitImage.color = new Color(0,0,0,0);

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        HP = startingHP;
        isDead = false;
    }

}
