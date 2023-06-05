using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ShelfActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;

    [Networked(OnChanged = nameof(OnBookChanged))]
    private bool greenBook {get; set;}
    [Networked(OnChanged = nameof(OnBookChanged))]
    private bool redBook {get; set;}

    [Networked]
    private bool missionCompleted {get; set;}

    public GameObject image1;
    public GameObject image2;
    public GameObject explosionParticleSystemPrefab;

    //other componet

    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                greenBook = false;
                redBook = false;
                missionCompleted = false;
            }
        }

        image1.SetActive(greenBook);
        image2.SetActive(redBook);
    }

    public override void action(Transform other)
    {
        Debug.Log("OnAction");

        if(other == null)
            return;
        
        PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
        if(playerActionHandler != null)
        {
            if(playerActionHandler.redBook)
            {
                this.redBook = true;
                playerActionHandler.redBook = false;
            }
            if(playerActionHandler.greenBook)
            {
                this.greenBook = true;
                playerActionHandler.greenBook = false;
            }
        }
        
    }

    static void OnBookChanged(Changed<ShelfActionHandler> changed)
    {
        Debug.Log($"GreenBook : {changed.Behaviour.greenBook}, RedBook : {changed.Behaviour.redBook}");
        //UI에 변경사항 표시
        changed.Behaviour.image1.SetActive(changed.Behaviour.greenBook);
        changed.Behaviour.image2.SetActive(changed.Behaviour.redBook);

        if(changed.Behaviour.Object.HasStateAuthority)
        {
            if(changed.Behaviour.greenBook && changed.Behaviour.redBook && !changed.Behaviour.missionCompleted)
                changed.Behaviour.RPC_MissionComplete();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MissionComplete()
    {
        Debug.Log("Mission Complete");
        Instantiate(explosionParticleSystemPrefab, transform.position, Quaternion.LookRotation(transform.forward));

        if(Object != null && Object.HasStateAuthority)
            missionCompleted = true;
    }

}
