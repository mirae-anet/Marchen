using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DoorActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;

    [Networked(OnChanged = nameof(OnValueChanged))]
    private bool Key {get; set;}

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

    static void OnValueChanged(Changed<DoorActionHandler> changed)
    {
        changed.Behaviour.KeyImage.SetActive(changed.Behaviour.Key);

        if(changed.Behaviour.Object.HasStateAuthority)
        {
            if(changed.Behaviour.Key && !changed.Behaviour.missionCompleted)
                changed.Behaviour.RPC_MissionComplete();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MissionComplete()
    {
        Debug.Log("Mission Complete");
        Instantiate(explosionParticleSystemPrefab, transform.position, Quaternion.LookRotation(transform.forward));
        StartCoroutine(OpenDoorCO());

        if(Object != null && Object.HasStateAuthority)
            missionCompleted = true;
    }

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
