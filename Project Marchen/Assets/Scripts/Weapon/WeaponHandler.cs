using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public GrenadeHandler grenadePrefab;
    public RocketHandler rocketPrefab;

    [Header("Effects")]
    public ParticleSystem fireParticleSystem;

    [Header("Anchor Point")]
    [SerializeField]
    Transform bodyAnchorPoint;

    [Header("Collision")]
    public LayerMask collisionLayer;

    [Header("Gun damage")]
    [SerializeField]
    byte damageAmount;
    
    //모두에게 공유되는 변수
    [Networked(OnChanged = nameof(OnFireChanged))] //값의 변화 -> 호출할 함수

    public bool isFiring {get; set;}

    float lastTimeFired = 0;

    [Header("Weapon settings")]
    [SerializeField]
    float gunCoolTime = 0.15f;
    [SerializeField]
    float grenadeCoolTime = 2.0f;
    [SerializeField]
    float rocketCoolTime = 2.0f;
    [SerializeField]
    float ThrowForce = 50f;

    //Timing (grenade)
    TickTimer grenadeFireDelay = TickTimer.None;
    TickTimer rocketFireDelay = TickTimer.None;

    //other component
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;

    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetComponent<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
    }
    void Start()
    {
        
    }

    //client는 InputAuthority를 가지고 server는 StateAuthority를 가진다.
    //동작이나 값을 동기화하려면 별도의 방법을 추가적으로 사용해야 한다. ex) RPC, [Networked], Base.~~~ 등
    public override void FixedUpdateNetwork()
    {
        if(hpHandler.GetIsDead())
            return;
        
        //inputAuthority가 있는 클라이언트와 전달 받은 서버만 실행한다.
        if(GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);
            if(networkInputData.isGrenadeFireButtonPressed)
                FireGrenade(networkInputData.aimForwardVector);
            if(networkInputData.isRocketLauncherFireButtonPressed)
                FireRocket(networkInputData.aimForwardVector);
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        if(Time.time - lastTimeFired < gunCoolTime)
            return;
        //inputAuthority가 있는 클라이언트와 전달 받은 서버만 실행한다.
        StartCoroutine(FireEffect());

        //발사 위치, 발사 방향, 발사 거리, 발사한 사람, 적중한 히트박스 정보, 상호작용할 레이어 마스크, 옵션 : physic object도 포함한다.(벽에 숨거나 등)
        Runner.LagCompensation.Raycast(bodyAnchorPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, collisionLayer, HitOptions.IncludePhysX);

        //for debuging
        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if(hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;
        
        if(hitinfo.Hitbox != null) // hit hitbox
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            if(Object.HasStateAuthority)
            {
                if(Object.InputAuthority == hitinfo.Hitbox.Root.GetComponent<NetworkObject>().InputAuthority)
                    return;
                //아니면 데미지
                hitinfo.Hitbox.transform.root.GetComponent<EnemyHPHandler>().OnTakeDamage(networkPlayer.nickName.ToString(), networkObject, damageAmount, transform.position);
            }

            isHitOtherPlayer = true;
        }
        else if(hitinfo.Collider != null) // hit physX collider
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.root.name}");
        }

        //debug
        if(isHitOtherPlayer)
            Debug.DrawRay(bodyAnchorPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(bodyAnchorPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        lastTimeFired = Time.time;

    }

    IEnumerator FireEffect()
    {
        isFiring = true;

        fireParticleSystem.Play(); //server and client who has inputAuthority

        yield return new WaitForSeconds(0.09f); //wait 0.09sec

        isFiring = false;
    }

    //The fuction called with [Networked(...)] must be static.
    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        bool isFiringCurrent = changed.Behaviour.isFiring; //isFiring이 아닌 changed.Behaviour.isFiring으로 접근

        //Load the old value
        changed.LoadOld();
        bool isFiringOld = changed.Behaviour.isFiring;

        if(isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();
    }

    // 다른 사람들에게도 보이게
    void OnFireRemote()
    {
        if(!Object.HasInputAuthority && !Object.HasStateAuthority)
            fireParticleSystem.Play();
    }

    void FireGrenade(Vector3 aimFowardVector)
    {
        //Check that we have not recently fired a grenade.
        if(grenadeFireDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(grenadePrefab, bodyAnchorPoint.position + aimFowardVector * 1.5f, Quaternion.LookRotation(aimFowardVector), Object.InputAuthority, (runner, spawnedGrenade) =>
            {
                spawnedGrenade.GetComponent<GrenadeHandler>().Throw(aimFowardVector * ThrowForce, Object.InputAuthority, networkObject, networkPlayer.nickName.ToString());
                Debug.Log("${Time.time} {networkPlayer.nickName} throw grenade");
            });

            //Start a new timer to avoid grenade spamming.
            grenadeFireDelay = TickTimer.CreateFromSeconds(Runner, grenadeCoolTime);
        }
    }
    void FireRocket(Vector3 aimFowardVector)
    {
        //Check that we have not recently fired a rocket.
        if(rocketFireDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(rocketPrefab, bodyAnchorPoint.position + aimFowardVector * 1.5f, Quaternion.LookRotation(aimFowardVector), Object.InputAuthority, (runner, spawnedRocket) =>
            {
                spawnedRocket.GetComponent<RocketHandler>().Fire(Object.InputAuthority, networkObject, networkPlayer.nickName.ToString());
            });

            //Start a new timer to avoid rocket spamming.
            rocketFireDelay = TickTimer.CreateFromSeconds(Runner, rocketCoolTime);
        }
    }
}
