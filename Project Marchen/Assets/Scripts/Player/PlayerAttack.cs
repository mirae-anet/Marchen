using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private PlayerController playerController;
    private WeaponMain weaponMain;

    private bool isRange = false;

    private float weaponDelay;

    public enum Type { Hammer, Gun, Staff };

    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject[] weapons;
    [SerializeField]
    private AudioSource shotSource;
    [SerializeField]
    private AudioClip shotClip;
    [SerializeField]
    private AudioSource reloadSource;
    [SerializeField]
    private AudioClip reloadClip;
    [SerializeField]
    private AudioSource swingSource;
    [SerializeField]
    private AudioClip swingClip;

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
                weapons[2].SetActive(false);
                weaponMain = weapons[0].GetComponent<WeaponMain>();
                break;

            case Type.Gun:
                weapons[0].SetActive(false);
                weapons[1].SetActive(true);
                weapons[2].SetActive(false);
                weaponMain = weapons[1].GetComponent<WeaponMain>();
                break;

            case Type.Staff:
                weapons[0].SetActive(false);
                weapons[1].SetActive(false);
                weapons[2].SetActive(true);
                weaponMain = weapons[2].GetComponent<WeaponMain>();
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
            if (weaponMain.GetCurAmmo() == 0)
            {
                playerController.SetIsAttack(false);
                DoReload();

                ReloadSound();
                yield break;
            }

            playerController.SetForward(camDir);
            anim.SetTrigger("doShot");

            ShotSound();
        }
        else
        {
            anim.SetTrigger("doSwing");

            SwingSound();
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
        ReloadSound();
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

    private void ShotSound()
    {
        shotSource.PlayOneShot(shotClip);
    }
    
    private void ReloadSound()
    {
        reloadSource.PlayOneShot(reloadClip);
    }

    private void SwingSound()
    {
        swingSource.PlayOneShot(swingClip);
    }
}