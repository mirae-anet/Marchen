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

    public void DoAttack(Vector3 attackDir)
    {
        StartCoroutine(AttackCoolTime(attackDir));
    }

    IEnumerator AttackCoolTime(Vector3 dir)
    {
        // 선딜레이
        //rigid.velocity = new Vector3((saveDir * moveSpeed).x, rigid.velocity.y, (saveDir * moveSpeed).z);
        anim.SetBool("isRun", true);

        yield return new WaitForSeconds(0.3f);

        //rigid.velocity = new Vector3((saveDir * moveSpeed).x * 0.3f, rigid.velocity.y, (saveDir * moveSpeed).z * 0.3f);
        if (isRange)
        {
            playerController.SetForward(dir);
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

        if (reloadInput && !playerController.GetActive())
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
