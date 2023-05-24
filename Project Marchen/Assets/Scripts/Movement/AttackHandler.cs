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
    WeaponHandler weaponHandler;

    void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
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
        StopReload();
        weaponHandler.Attack(aimDir);
        Debug.Log("DoAttack");
    }

    public void DoReload()
    {
        if (weaponHandler.type == WeaponHandler.Type.Melee)
            return;
        weaponHandler.Reload();
        Debug.Log("DoReload");
    }

    public void StopReload()
    {
        weaponHandler.StopReload();
    }

    //추가
    public void ChangeWeapon(int weaponIndex)
    {
        weaponType = (Type)weaponIndex;
        WeaponEquip();
    }

}
