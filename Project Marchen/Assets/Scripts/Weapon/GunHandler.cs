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
    public Type type = Type.Range;

    [Range(1f, 100f)]
    public byte damageAmount = 25;
    
    [Range(0f, 5f)]
    public float delay = 0.5f;

    [Range(1, 100)]
    public int maxAmmo = 30;

    [Range(0, 100)]
    public int curAmmo = 30;

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

    public override void Attack(Vector3 aimDir)
    {
        if (curAmmo <= 0)
        {
            networkPlayerController.SetIsAttack(false);
            DoReload();
            return;
        }

        networkPlayerController.RPC_LookForward(aimDir);
        anim.SetTrigger("doShot");

        // weaponMain.Attack();
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

        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);

        yield return new WaitForSeconds(delay);

        networkPlayerController.SetIsAttack(false);
    }

    public override void DoReload(){}
    public override void StopReload(){}

    IEnumerator ReloadOut(float time)
    {
        yield return new WaitForSeconds(time);

        weaponMain.SetCurAmmo(weaponMain.GetMaxAmmo());
        playerController.SetIsReload(false);
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetBool(string action, bool isDone)
    {
        anim.SetBool(action, isDone);
    }

    [Rpc (RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
}