using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
public class AttackHandler : NetworkBehaviour
{
    public enum Type { Hammer, Gun };
    
    [Networked(OnChanged = nameof(WeaponEquip))]
    public Type weaponType { get; set; }

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    //other componet
    HPHandler hpHandler;
    WeaponHandler weaponHandler;

    Scene scene;

    void Start()
    {

        if (SceneManager.GetActiveScene().name == "TestScene(network)_Potal")
        {
            if (Object.HasStateAuthority)
            {
                if (weaponType == Type.Gun)
                {
                    weaponType = Type.Hammer;
                }
                else
                {
                    weaponType = Type.Gun;
                }
            }
        }
        else
        {
            return;
        }

        hpHandler = GetComponent<HPHandler>();


    }

    static void WeaponEquip(Changed<AttackHandler> changed)
    {
        switch (changed.Behaviour.weaponType)
        {
            case Type.Hammer:
                changed.Behaviour.weapons[0].SetActive(true);
                changed.Behaviour.weapons[1].SetActive(false);
                changed.Behaviour.weaponHandler = changed.Behaviour.weapons[0].GetComponent<WeaponHandler>();
                break;

            case Type.Gun:
                changed.Behaviour.weapons[0].SetActive(false);
                changed.Behaviour.weapons[1].SetActive(true);
                changed.Behaviour.weaponHandler = changed.Behaviour.weapons[1].GetComponent<WeaponHandler>();
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
    //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    //추가
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
