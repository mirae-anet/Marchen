using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 플레이어의 상호작용을 담당하는 클래스
public class PlayerActionHandler : InteractionHandler
{
    public bool skipSettingStartValues = false;

    /// @brief 상호작용 가능한 대상의 레이어값.
    public LayerMask layerMask;

    /// @brief 상호작용 범위(박스)를 설정하는 기준점.
    public Transform BodyAnchor;

    /// @brief 상호작용 범위(박스)의 크기를 설정. 
    Vector3 boxSize = new Vector3(2,2,2);

    /// @brief 플레이어가 Key를 가지고 있는지.
    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool Key {get; set;}

    /// @brief 플레이어가 GreenBattery를 가지고 있는지.
    [Networked(OnChanged = nameof(OnValueChanged))]
    public bool GreenBattery {get; set;}

    /// @brief 플레이어가 BlueBattery를 가지고 있는지.
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

    /// @brief 플레이어와의 상호작용을 위해서 호출하는 메서드
    /// @details 호출한 게임 오브젝트(other)에 따라서 다른 동작을 수행.
    /// @see DespawnAction, PickUpAction
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

    /// @brief f를 눌러서 상호작용을 시도할 때 호출하는 메서드.
    /// @details 일정한 크기의 박스(OverlapBox)를 생성한다. 박스 안에 상호작용 가능한 레이어가 존재하면 해당 오브젝트와 상호작용 시작(action() 호출).
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

    /// @brief 아이템을 습득한 경우 실행되는 콜백함수.
    /// @details 획득한 아이템을 UI에 표시.
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

    /// @brief 아이템 습득 소리가 다른 컴퓨터에서도 실행되도록 함.
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AudioPlay(string audioType)
    {
        pickUpSound.Play();
    }
}
