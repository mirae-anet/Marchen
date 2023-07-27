using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @breif 1 스테이지의 시계와의 상호작용을 위한 클래스
public class ClockActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;

    /// @breif 파란색 배터리의 유무.
    [Networked(OnChanged = nameof(OnBatteryChanged))]
    private bool BlueBattery {get; set;}
    /// @breif 초록색 배터리의 유무.
    [Networked(OnChanged = nameof(OnBatteryChanged))]
    private bool GreenBattery {get; set;}

    /// @breif 미션 성공 여부. 동기화 되어있음.
    [Networked]
    private bool missionCompleted {get; set;}

    public GameObject Blue;
    public GameObject Green;
    public GameObject MinuteHand;
    public GameObject MinuteHand_Next;
    public GameObject explosionParticleSystemPrefab;
    /// @breif 길을 막고 있는 장애물.
    public GameObject Block;
    public AudioSource batteryEquipSound;

    //other componet

    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                GreenBattery = false;
                BlueBattery = false;
                missionCompleted = false;
            }
        }

        Green.SetActive(GreenBattery);
        Blue.SetActive(BlueBattery);
        MinuteHand.SetActive(!missionCompleted);
        MinuteHand_Next.SetActive(missionCompleted);
        Block.SetActive(!missionCompleted);
    }

    /// @breif 시계와의 상호작용을 위하여 호출하는 메서드.
    /// @details 플레이어가 배터리를 가지고 상호작용하면 해당 배터리를 시계에 장착할 수 있음.
    public override void action(Transform other)
    {
        Debug.Log("OnAction");

        if(other == null)
            return;
        
        PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
        if(playerActionHandler != null)
        {
            if(playerActionHandler.BlueBattery)
            {
                this.BlueBattery = true;
                playerActionHandler.BlueBattery = false;
            }
            if(playerActionHandler.GreenBattery)
            {
                this.GreenBattery = true;
                playerActionHandler.GreenBattery = false;
            }
        }
        
    }

    /// @breif BlueBattery와 GreenBattery의 값이 변하면 호출되는 콜백.
    /// @details 각종 효과(배터리 배치, 효과음) 및 미션 달성 여부를 검사.
    static void OnBatteryChanged(Changed<ClockActionHandler> changed)
    {
        changed.Behaviour.Green.SetActive(changed.Behaviour.GreenBattery);
        changed.Behaviour.Blue.SetActive(changed.Behaviour.BlueBattery);
        changed.Behaviour.batteryEquipSound.Play();

        if(changed.Behaviour.Object.HasStateAuthority)
        {
            if(changed.Behaviour.GreenBattery && changed.Behaviour.BlueBattery && !changed.Behaviour.missionCompleted)
                changed.Behaviour.RPC_MissionComplete();
        }
    }

    /// @breif 파란색, 초록색 배터리를 모두 장착하면 실행할 동작들.
    /// @details 시각적인 효과들(연기, 초침, 분침), 장애물 비활성화.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MissionComplete()
    {
        Debug.Log("Mission Complete");
        Instantiate(explosionParticleSystemPrefab, transform.position, Quaternion.LookRotation(transform.forward));

        MinuteHand.SetActive(false);
        MinuteHand_Next.SetActive(true);
        Block.SetActive(false);

        if(Object != null && Object.HasStateAuthority)
            missionCompleted = true;
    }

}
