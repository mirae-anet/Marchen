using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;
    public LayerMask layerMask;
    public Transform BodyAnchor;
    Vector3 boxSize = new Vector3(2,2,2);

    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool Key {get; set;}

    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool GreenBattery {get; set;}
    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool BlueBattery {get; set;}

    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool greenBook {get; set;}
    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool redBook {get; set;}

    //other component
    NetworkPlayerController networkPlayerController;
    public GameObject image1;
    public GameObject image2;
    public GameObject BlueBatteryImage;
    public GameObject GreenBatteryImage;
    public GameObject KeyImage;
    // public GameObject Key;
    public GameObject myImage1;
    public GameObject myImage2;
    public GameObject myBlueBatteryImage;
    public GameObject myGreenBatteryImage;
    public GameObject myKeyImage;
    public AudioSource pickUpSound;
    // public GameObject myKey;

    void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                BlueBattery = false;
                GreenBattery = false;
                greenBook = false;
                redBook = false;
                Key = false;
            }
        }

        BlueBatteryImage.SetActive(BlueBattery);
        GreenBatteryImage.SetActive(GreenBattery);
        myBlueBatteryImage.SetActive(BlueBattery);
        myGreenBatteryImage.SetActive(GreenBattery);
        KeyImage.SetActive(Key);
        myKeyImage.SetActive(Key);

        image1.SetActive(greenBook);
        image2.SetActive(redBook);
        myImage1.SetActive(greenBook);
        myImage2.SetActive(redBook);

        networkPlayerController = GetComponent<NetworkPlayerController>();
    }

    public override void action(Transform other)
    {
        if(!Object.HasStateAuthority)
            return;

        if(other.TryGetComponent<DespawnAction>(out DespawnAction despawnAction))
        {
            EnemyHPHandler[] enemyHPHandlers = FindObjectsOfType<EnemyHPHandler>();
            if(enemyHPHandlers.Length > 0)
            {
                for(int i = 0; i < enemyHPHandlers.Length; i++)
                {
                    if(enemyHPHandlers[i] != null)
                    {
                        enemyHPHandlers[i].OnTakeDamage("", Object, 255, transform.position); //MAX 즉사
                    }
                }
            }
        }
        else if(other.TryGetComponent<PickUpAction>(out PickUpAction pickUpAction))
        {
            switch (pickUpAction.type)
            {
                case PickUpAction.Type.Key:
                    Key = true;
                    break;

                case PickUpAction.Type.BlueBattery:
                    BlueBattery = true;
                    break;

                case PickUpAction.Type.GreenBattary:
                    GreenBattery = true;
                    break;

                case PickUpAction.Type.GreenBook:
                    greenBook = true;
                    break;

                case PickUpAction.Type.RedBook:
                    redBook = true;
                    break;
            }
            RPC_AudioPlay("pickUp");
        }
    }

    public void Interact()
    {
        Collider[] colliders  = Physics.OverlapBox(BodyAnchor.position, boxSize/2, Quaternion.LookRotation(transform.forward), layerMask);
        Debug.Log("Interact : " + colliders.Length);
        if(colliders.Length > 0)
        {
            for(int i = 0; i < colliders.Length; i++)
            {
                InteractionHandler interactionHandler = colliders[i].GetComponent<InteractionHandler>();

                if(interactionHandler != null)
                    interactionHandler.action(transform);
            }
        }
    }

    static void OnValueChanged(Changed<PlayerActionHandler> changed)
    {
        changed.Behaviour.KeyImage.SetActive(changed.Behaviour.Key);
        changed.Behaviour.myKeyImage.SetActive(changed.Behaviour.Key);

        changed.Behaviour.BlueBatteryImage.SetActive(changed.Behaviour.BlueBattery);
        changed.Behaviour.GreenBatteryImage.SetActive(changed.Behaviour.GreenBattery);
        changed.Behaviour.myBlueBatteryImage.SetActive(changed.Behaviour.BlueBattery);
        changed.Behaviour.myGreenBatteryImage.SetActive(changed.Behaviour.GreenBattery);

        changed.Behaviour.image1.SetActive(changed.Behaviour.greenBook);
        changed.Behaviour.image2.SetActive(changed.Behaviour.redBook);
        changed.Behaviour.myImage1.SetActive(changed.Behaviour.greenBook);
        changed.Behaviour.myImage2.SetActive(changed.Behaviour.redBook);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AudioPlay(string audioType)
    {
        pickUpSound.Play();
    }
}
