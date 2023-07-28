using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 1 스테이지의 문과 상호작용을 위한 클래스
public class DoorActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;
    /// @brief 열쇠의 유무. 동기화 되어있음.
    [Networked(OnChanged = nameof(OnValueChanged))]
    private bool Key {get; set;}
    /// @brief 미션 성공 여부. 동기화 되어있음.
    [Networked]
    private bool missionCompleted {get; set;}

    //other componet
    public GameObject doorWing;
    public GameObject KeyImage;
    public GameObject explosionParticleSystemPrefab;
    public AudioSource openingSound;


    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                Key = false;
                missionCompleted = false;
            }
        }

        KeyImage.SetActive(Key);
        // door rotate
        if(missionCompleted)
            StartCoroutine(OpenDoorCO());
    }
    /// @brief 문과 상호작용하기 위하여 호출하는 메서드.
    /// @details 플레이어가 열쇠를 가지고서 상호작용하면 해당 문을 열 수 있다.
    public override void action(Transform other)
    {
        Debug.Log("OnAction");

        if(other == null)
            return;
        
        PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
        if(playerActionHandler != null)
        {
            if(playerActionHandler.Key)
            {
                this.Key = true;
                playerActionHandler.Key = false;
            }
        }
    }

    /// @brief Key의 값이 변하면 호출되는 콜백.
    static void OnValueChanged(Changed<DoorActionHandler> changed)
    {
        changed.Behaviour.KeyImage.SetActive(changed.Behaviour.Key);

        if(changed.Behaviour.Object.HasStateAuthority)
        {
            if(changed.Behaviour.Key && !changed.Behaviour.missionCompleted)
                changed.Behaviour.RPC_MissionComplete();
        }
    }

    /// @brief OnValueChanged에서 호출되는 RPC. 모든 컴퓨터에서 실행.
    /// @see DoorActionHandler.OnValueChanged
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MissionComplete()
    {
        Debug.Log("Mission Complete");
        Instantiate(explosionParticleSystemPrefab, transform.position, Quaternion.LookRotation(transform.forward));
        StartCoroutine(OpenDoorCO());

        if(Object != null && Object.HasStateAuthority)
            missionCompleted = true;
    }

    /// @brief 시,청각적인 효과.
    IEnumerator OpenDoorCO()
    {
        openingSound.Play();
        while(doorWing.transform.localRotation.eulerAngles.y < 90)
        {
            Vector3 oldRotation = doorWing.transform.rotation.eulerAngles;
            Quaternion newRotation = Quaternion.Euler(oldRotation.x, oldRotation.y + 5f, oldRotation.z);
            doorWing.transform.rotation = (newRotation);
            yield return new WaitForSeconds(0.01f);
        }
    }

}
