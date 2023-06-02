using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
public class AttackHandler : NetworkBehaviour
{
    public enum Type { Hammer, Gun };
    [Header("설정")]//시작
    private Type weaponType;

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    //other componet
    HPHandler hpHandler;
    WeaponHandler weaponHandler;

    Scene scene;
    void Start()
    {
        hpHandler = GetComponent<HPHandler>();

        if (scene.name == "TestScene(network)_Potal")
            ChangeWeapon(1);

        // 선택했던 weaponType 값
        if (PlayerPrefs.HasKey("WeaponType"))
        {
            weaponType = (Type)PlayerPrefs.GetInt("WeaponType");
        }

        WeaponEquip();

        // 씬 변경 시 이전 값을 유지하기 위해 추가
        if (Object.HasStateAuthority)
        {
            //RPC_RequestWeaponChange((int)weaponType);
            ChangeWeapon((int)weaponType);
        }
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
    //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    //추가
    public void ChangeWeapon(int weaponIndex)
    {
        weaponType = (Type)weaponIndex;
        WeaponEquip();

        if (Object.HasInputAuthority)
        {
            RPC_RequestWeaponChange(weaponIndex);
        }

        // 변경한값 저장
        PlayerPrefs.SetInt("WeaponType", (int)weaponType);
    }



    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_RequestWeaponChange(int weaponIndex)
    {
        weaponType = (Type)weaponIndex;
        WeaponEquip();
    }

}
