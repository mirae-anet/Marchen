using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AttackHandler : NetworkBehaviour
{
    public enum Type { Hammer, Gun };
    [Header("설정")]
    public Type weaponType;

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    //other componet
    HPHandler hpHandler;
    Animator anim;
    WeaponHandler weaponHandler;

    void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        anim = GetComponentInChildren<Animator>();
        WeaponEquip();
    }

    void WeaponEquip()
    {
        switch (weaponType)
        {
            case Type.Hammer:
                weapons[0].SetActive(true);
                weapons[1].SetActive(false);
                weaponHandler = weapons[0].GetComponent<WeaponHandler>();
                break;

            case Type.Gun:
                weapons[0].SetActive(false);
                weapons[1].SetActive(true);
                weaponHandler = weapons[1].GetComponent<WeaponHandler>();
                break;
        }
    }

    public void DoAttack(Vector3 aimDir)
    {
        weaponHandler.StopReload();
        weaponHandler.Attack(aimDir);
    }

    /*
    public void DoReload()
    {
        // if (weaponMain.type == WeaponMain.Type.Melee)
        //     return;
        
        anim.SetTrigger("doReload");
        playerController.SetIsReload(true);

        StartCoroutine("ReloadOut", 0.8f);
    }


    public void StopReload()
    {
        StopCoroutine("ReloadOut");
        playerController.SetIsReload(false);
    }
    */

}
