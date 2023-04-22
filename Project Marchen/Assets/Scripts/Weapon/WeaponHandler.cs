using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    //모두에게 공유되는 변수
    [Networked(OnChanged = nameof(OnFireChanged))] //값의 변화 -> 호출할 함수
    public bool isFiring {get; set;}

    public ParticleSystem fireParticleSystem;
    public Transform aimPoint;
    public LayerMask collisionLayer;
    
    float lastTimeFired = 0;

    [SerializeField]
    float coolTime = 0.15f;

    //other component
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }
    void Start()
    {
        
    }

    //client는 InputAuthority를 가지고 server는 StateAuthority를 가진다.
    //FixedUpdateNetwork에서 값을 변경하려면 StateAuthority가 필요한 듯?
    //FixedUpdateNetwork는 실행시간을 동기화하는듯. 동작이나 값을 동기화하려면 별도의 방법을 추가적으로 사용해야 한다. ex) RPC, [Networked], Base.~~~ 등
    public override void FixedUpdateNetwork()
    {
        if(hpHandler.isDead)
            return;
        
        //Get the input from the network
        //inputAuthority가 있는 클라이언트와 전달 받은 서버만 실행한다.
        if(GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if(Time.time - lastTimeFired < coolTime)
            return;
        //유니티에서 코루틴은 실행을 일시 중단하고 나중에 중단한 지점부터 다시 실행할 수 있는 특별한 종류의 함수입니다.
        //inputAuthority가 있는 클라이언트와 전달 받은 서버만 실행한다.
        StartCoroutine(FireEffect());

        //발사 위치, 발사 방향, 발사 거리, 발사한 사람, 적중한 히트박스 정보, 상호작용할 레이어 마스크, 옵션
        //옵션: physic object도 포함한다.(벽에 숨거나 등)
        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, collisionLayer, HitOptions.IncludePhysX);
        //옵션 : 자기자신은 무시
        // Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, collisionLayer, HitOptions.IgnoreInputAuthority);

        //for debuging
        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if(hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;
        
        if(hitinfo.Hitbox != null) // hit hitbox
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            // if you can change the value of the other players.
            //일반적으로 서버가 상태 권한을 가지고 있으며, 클라이언트는 그에 따라 상태를 갱신합니다.
            if(Object.HasStateAuthority)
            {
                //공격한 사람과 맞은 사람이 같다면 return
                if(Object.InputAuthority == hitinfo.Hitbox.Root.GetComponent<NetworkObject>().InputAuthority)
                    return;
                //아니면 데미지
                hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(networkPlayer.nickName.ToString());
            }

            isHitOtherPlayer = true;
        }
        else if(hitinfo.Collider != null) // hit physX collider
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.root.name}");
        }

        //debug
        if(isHitOtherPlayer)
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        lastTimeFired = Time.time;
        
    }

    IEnumerator FireEffect()
    {
        isFiring = true;

        // Debug.Log($"{Time.time} FireEffect");
        fireParticleSystem.Play(); //server and client who has inputAuthority

        yield return new WaitForSeconds(0.09f); //wait 0.09sec

        isFiring = false;
    }

    //The fuction called with [Networked(...)] must be static.
    //Networked된 variable를 수정한 StateAuthority를 가진 Server 빼고 모두 다 실행한다. InputAuthority를 가진고 있어도 실행한다.
    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFiredChaged value {changed.Behaviour.isFiring}");
        bool isFiringCurrent = changed.Behaviour.isFiring; //isFiring이 아닌 changed.Behaviour.isFiring으로 접근

        //Load the old value
        changed.LoadOld();
        bool isFiringOld = changed.Behaviour.isFiring;

        if(isFiringCurrent && !isFiringOld)
        {
            changed.Behaviour.OnFireRemote();
        }
    }

    // 다른 사람들에게도 보이게
    void OnFireRemote()
    {
        if(!Object.HasInputAuthority && !Object.HasStateAuthority)
        {
            // Debug.Log($"{Time.time} OnFireRemote");
            fireParticleSystem.Play();
        }
    }
}
