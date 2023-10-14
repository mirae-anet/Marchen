using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

//@brief 플레이어의 무기 변경에 관련된 스크립트
public class AttackHandler : NetworkBehaviour
{

    //@brief 무기 타입 정의
    public enum Type { Hammer, Gun, Staff};
    
    [Networked(OnChanged = nameof(OnChangeWeapon))]
    public Type weaponType { get; set; }

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    //other componet
    WeaponHandler weaponHandler;

    //@brief 기본 무기 설정
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Scene_2")
        {
            if (Object.HasStateAuthority)
            {
                weaponType = Type.Hammer;
            }
        }

        WeaponEquip();
    }

    //@brief 캐릭터 무기 변환
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
            case Type.Staff:
                weapons[2].SetActive(true);
                weaponHandler = weapons[2].GetComponent<WeaponHandler>();
                break;
        }
    }

    //@brief 무기 타입 변경시 호출
    static void OnChangeWeapon(Changed<AttackHandler> changed)
    {
        changed.Behaviour.WeaponEquip();
    }

    public void DoAttack(Vector3 aimDir)
    {
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

    //@brief 플레이어의 무기를 알맞은 타입으로 변경
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
