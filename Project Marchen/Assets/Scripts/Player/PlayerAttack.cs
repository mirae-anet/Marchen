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

    private bool attackInput;
    private bool reloadInput;

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

        weaponDelay = weaponMain.getDelay();
    }

    public void DoAttack(Vector3 attackDir)
    {
        StartCoroutine(AttackCoolTime(attackDir));
    }

    IEnumerator AttackCoolTime(Vector3 Dir)
    {
        // 선딜레이
        //rigid.velocity = new Vector3((saveDir * moveSpeed).x, rigid.velocity.y, (saveDir * moveSpeed).z);
        anim.SetBool("isRun", true);

        yield return new WaitForSeconds(0.3f);

        //rigid.velocity = new Vector3((saveDir * moveSpeed).x * 0.3f, rigid.velocity.y, (saveDir * moveSpeed).z * 0.3f);
        weaponMain.Attack();
        anim.SetTrigger(weaponMain.GetWeaponType() == WeaponMain.Type.Melee ? "doSwing" : "doShot");

        yield return new WaitForSeconds(weaponDelay);

        playerController.setIsAttack(false);
    }

    private void Reload()
    {
        if (weaponMain == null)
            return;

        if (weaponMain.type == WeaponMain.Type.Melee)
            return;
        /*
        if (ammo == 0)  // 총알이 0개일 때
            return;
        */
        if (reloadInput && !playerController.getActive())
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2f);
        }
    }

    private void ReloadOut()
    {
        //int reAmmo = reAmmo < weaponMain.maxAmmo ? ammo : weaponMain.maxAmmo;
        //weaponMain.curAmmo = reAmmo;

        //ammo -= reAmmo;
        isReload = false;
    }

}
