using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

/// @brief 플레이어 공격 관련 클래스
public class AttackHandler : NetworkBehaviour
{
    public enum Type { Hammer, Gun, Tracker};
    
    /// @brief 사용하는 무기 종류
    /// @details default는 해머. networked되어있음. 무기 변경시 OnChangeWeapon() 호출.
    /// @see OnChangeWeapon()
    [Networked(OnChanged = nameof(OnChangeWeapon))]
    public Type weaponType { get; set; }

    /// @brief 사용할 수 있는 무기들의 배열
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

    /// @brief 무기를 장착.
    void WeaponEquip()
    {
        foreach(GameObject weapon in weapons){
            weapon.SetActive(false);
        }
        switch (weaponType)
        {
            case Type.Hammer:
                weapons[0].SetActive(true);
                weaponHandler = weapons[0].GetComponent<WeaponHandler>();
                break;

            case Type.Gun:
                weapons[1].SetActive(true);
                weaponHandler = weapons[1].GetComponent<WeaponHandler>();
                break;
            case Type.Tracker:
                weapons[2].SetActive(true);
                weaponHandler = weapons[2].GetComponent<WeaponHandler>();
                break;
        }
    }

    /// @brief 무기(weaponType) 변경시 호출됨.
    static void OnChangeWeapon(Changed<AttackHandler> changed)
    {
        changed.Behaviour.WeaponEquip();
    }

    /// @brief 공격 실행
    /// @see WeaponHandler
    public void DoAttack(Vector3 aimDir)
    {
        weaponHandler.Attack(aimDir);
        Debug.Log("DoAttack");
    }

    /// @brief 재장전 실행
    /// @details 재장전이 필요하지 않은 무기라면 아무런 동작이 수행되지않음. 
    public void DoReload()
    {
        if (weaponHandler.type == WeaponHandler.Type.Melee)
            return;
        weaponHandler.Reload();
        Debug.Log("DoReload");
    }

    /// @brief 재장전 중단
    /// @see WeaponHandler.StopReload()
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
