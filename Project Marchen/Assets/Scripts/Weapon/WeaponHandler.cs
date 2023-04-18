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
    
    float lastTimeFired = 0;

    [SerializeField]
    float coolTime = 0.15f;

    void Start()
    {
        
    }

    //FixedUpdateNetwork에서 게임 오브젝트에 영향을 주거나 컴포넌트를 이용하는 작업은 InputAuthority가 있는 local에서 처리하는듯.
    public override void FixedUpdateNetwork()
    {
        //Get the input from the network
        if(GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);
        }
    }

    //local only
    void Fire(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if(Time.time - lastTimeFired < coolTime)
            return;
        //유니티에서 코루틴은 실행을 일시 중단하고 나중에 중단한 지점부터 다시 실행할 수 있는 특별한 종류의 함수입니다.
        StartCoroutine(FireEffect());

        lastTimeFired = Time.time;
        
    }

    //loacl only
    IEnumerator FireEffect()
    {
        isFiring = true;

        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f); //wait 0.09sec

        isFiring = false;
    }

    //The fuction called with [Networked(...)] must be static.
    //everyone
    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        Debug.Log($"{Time.time} OnFiredChaged value {changed.Behaviour.isFiring}");
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
        if(!Object.HasInputAuthority)
        {
            fireParticleSystem.Play();
        }
    }
}