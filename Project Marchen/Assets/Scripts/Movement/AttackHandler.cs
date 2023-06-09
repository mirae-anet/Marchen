using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
public class AttackHandler : NetworkBehaviour
{
    public enum Type { Hammer, Gun };
    
    [Networked(OnChanged = nameof(OnChangeWeapon))]
    public Type weaponType { get; set; }

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    //other componet
    WeaponHandler weaponHandler;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "TestScene(network)_Potal")
        {
            if (Object.HasStateAuthority)
            {
                weaponType = Type.Hammer;
            }
        }

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

    static void OnChangeWeapon(Changed<AttackHandler> changed)
    {
        changed.Behaviour.WeaponEquip();
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

    public void ChangeWeapon(int weaponIndex)
    {
        RPC_RequestWeaponChange(weaponIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestWeaponChange(int weaponIndex)
    {
        weaponType = (Type)weaponIndex;
    }

}
