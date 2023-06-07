using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ClockActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;

    [Networked(OnChanged = nameof(OnBatteryChanged))]
    private bool BlueBattery {get; set;}
    [Networked(OnChanged = nameof(OnBatteryChanged))]
    private bool GreenBattery {get; set;}

    [Networked]
    private bool missionCompleted {get; set;}

    public GameObject Blue;
    public GameObject Green;
    public GameObject MinuteHand;
    public GameObject MinuteHand_Next;
    // public GameObject Block;
    public GameObject explosionParticleSystemPrefab;

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
        // Block.SetActive(!missionCompleted);
    }

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

    static void OnBatteryChanged(Changed<ClockActionHandler> changed)
    {
        changed.Behaviour.Green.SetActive(changed.Behaviour.GreenBattery);
        changed.Behaviour.Blue.SetActive(changed.Behaviour.BlueBattery);

        if(changed.Behaviour.Object.HasStateAuthority)
        {
            if(changed.Behaviour.GreenBattery && changed.Behaviour.BlueBattery && !changed.Behaviour.missionCompleted)
                changed.Behaviour.RPC_MissionComplete();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MissionComplete()
    {
        Debug.Log("Mission Complete");
        Instantiate(explosionParticleSystemPrefab, transform.position, Quaternion.LookRotation(transform.forward));

        MinuteHand.SetActive(false);
        MinuteHand_Next.SetActive(true);
        // Block.SetActive(false);

        if(Object != null && Object.HasStateAuthority)
            missionCompleted = true;
    }

}
