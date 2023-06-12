using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMain : MonoBehaviour
{
    public enum Type { Melee, Range };

    [Header("오브젝트 연결")]
    [SerializeField]
    private SphereCollider meleeArea;
    [SerializeField]
    private TrailRenderer trailEffect;
    [SerializeField]
    private Transform bulletPos;
    [SerializeField]
    private Transform bulletCasePos;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private GameObject bulletCase;

    [Header("설정")]
    public Type type;
    [Range(1f, 100f)]
    public int damage = 25;
    [Range(0f, 5f)]
    public float delay = 0.35f;
    [Range(1, 100)]
    public int maxAmmo = 30;
    [Range(0, 100)]
    public int curAmmo = 30;

    public void Attack()
    {
        if (type == Type.Melee)
        {
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();

        bulletRigid.velocity = bulletPos.forward * 50;
        
        yield return null;

        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();

        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }

    public float GetDelay()
    {
        return delay;
    }

    public Type GetWeaponType()
    {
        return type;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    public int GetCurAmmo()
    {
        return curAmmo;
    }

    public void SetCurAmmo(int ammo)
    {
        curAmmo = ammo;
    }
}
