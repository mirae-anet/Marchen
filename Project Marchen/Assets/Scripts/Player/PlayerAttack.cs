using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;
    private PlayerController playerController;
    private WeaponMain weaponMain;

    private bool isReload = false;
    private bool isAttack = false;
    private bool isRange = false;

    private bool attackInput;
    private bool reloadInput;

    private float weaponDelay;
    private int ammo;

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
        rigid = GetComponent<Rigidbody>();
        WeaponEquip();
    }

    void Update()
    {
        Reload();
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
        StartCoroutine(AttackCoolTime(camDir));
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

    private void Reload()
    {
        if (weaponMain == null)
            return;

        if (weaponMain.type == WeaponMain.Type.Melee)
            return;

        if (ammo == 0)  // 총알이 0개일 때
            return;

        if (reloadInput && !playerController.GetActiveJDA())
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2f);
        }
    }

    private void ReloadOut()
    {
        int reAmmo = ammo < weaponMain.maxAmmo ? ammo : weaponMain.GetMaxAmmo();
        weaponMain.SetCurAmmo(reAmmo);

        ammo -= reAmmo;
        isReload = false;
    }

}
