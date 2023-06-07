using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GunHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    [SerializeField]
    private Transform bulletPos;
    [SerializeField]
    private Transform bulletCasePos;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private GameObject bulletCase;

    [Header("설정")]

    [Range(1f, 100f)]
    public int damageAmount = 5;
    
    [Range(0f, 5f)]
    public float delay = 0.5f;
    public float reloadTime = 0.8f;

    [Range(1, 100)]
    public int maxAmmo = 15;

    [Range(0, 100)]
    public int curAmmo = 15;

    //other compomponet
    Animator anim;
    NetworkPlayerController networkPlayerController;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
        networkPlayerController = GetComponentInParent<NetworkPlayerController>();
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        networkObject = GetComponentInParent<NetworkObject>();
    }
    private void Start() 
    {
        type = Type.Range;
    }

    public override void Attack(Vector3 aimDir)
    {
        if (curAmmo <= 0)
        {
            networkPlayerController.SetIsAttack(false);
            Reload();
            return;
        }

        networkPlayerController.RPC_LookForward(aimDir);
        RPC_animatonSetTrigger("doShot");

        curAmmo--;
        StartCoroutine("Shot");
    }

    IEnumerator Shot()
    {
        Vector3 aimForwardVector = networkPlayerController.aimForwardVector;
        Runner.Spawn(bullet, bulletPos.position, Quaternion.LookRotation(aimForwardVector), Object.InputAuthority, (runner, spawnedBullet) =>
        {
            spawnedBullet.GetComponent<BulletHandler>().Fire(Object.InputAuthority, networkObject, networkPlayer.nickName.ToString());
        });

        RPC_bulletCase();

        yield return new WaitForSeconds(delay);

        networkPlayerController.SetIsAttack(false);
    }

    public override void Reload()
    {
        RPC_animatonSetTrigger("doReload");
        networkPlayerController.SetIsReload(true);
        StartCoroutine(ReloadOut(reloadTime));
    }
    public override void StopReload()
    {
        StopCoroutine("ReloadOut");
        networkPlayerController.SetIsReload(false);
    }

    IEnumerator ReloadOut(float time)
    {
        yield return new WaitForSeconds(time);
        curAmmo = maxAmmo;
        networkPlayerController.SetIsReload(false);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_bulletCase()
    {
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}