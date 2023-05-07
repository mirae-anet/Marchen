using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private PlayerController playerController;
    private WeaponMain weaponMain;

    private bool isRange = false;

    private float weaponDelay;

    public enum Type { Hammer, Gun };

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;

    [Header("설정")]
    public Type weaponType;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
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
                weaponMain = weapons[0].GetComponent<WeaponMain>();
                break;

            case Type.Gun:
                weapons[0].SetActive(false);
                weapons[1].SetActive(true);
                weaponMain = weapons[1].GetComponent<WeaponMain>();
                break;
        }

        weaponDelay = weaponMain.GetDelay();

        if (weaponMain.GetWeaponType() == WeaponMain.Type.Range)
            isRange = true;
    }

    public void DoAttack(Vector3 camDir)
    {
        StartCoroutine("AttackCoolTime", camDir);
    }

    IEnumerator AttackCoolTime(Vector3 camDir)
    {
        if (isRange)
        {
            playerController.SetForward(camDir);
            anim.SetTrigger("doShot");
        }
        else
        {
            anim.SetTrigger("doSwing");
        }
        
        weaponMain.Attack();

        yield return new WaitForSeconds(weaponDelay);

        playerController.SetIsAttack(false);
    }

    public void DoReload()
    {
        if (weaponMain.type == WeaponMain.Type.Melee)
            return;
        
        anim.SetTrigger("doReload");
        playerController.SetIsReload(true);

        StartCoroutine("ReloadOut", 0.8f);
    }

    IEnumerator ReloadOut(float time)
    {
        yield return new WaitForSeconds(time);

        weaponMain.SetCurAmmo(weaponMain.GetMaxAmmo());
        playerController.SetIsReload(false);
    }

    public void StopReload()
    {
        StopCoroutine("ReloadOut");
        playerController.SetIsReload(false);
    }
}
