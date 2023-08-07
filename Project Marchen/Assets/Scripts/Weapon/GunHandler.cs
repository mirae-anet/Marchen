using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 총의 기능을 구현하는 클래스
public class GunHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    [SerializeField]
    /// @brief 총알이 발사되는 기준점
    private Transform bulletPos;
    [SerializeField]
    /// @brief 탄피가 배출되는 기준점
    private Transform bulletCasePos;
    [SerializeField]
    /// @brief 발사할 총알
    private GameObject bullet;
    [SerializeField]
    /// @brief 배출할 탄피
    private GameObject bulletCase;

    [Header("설정")]

    [Range(0f, 5f)]
    /// @brief 다음번 공격이 나가는데까지 걸리는 시간.
    public float delay = 1f;
    /// @brief 재장전하는데 필요한 시간
    public float reloadTime = 0.8f;

    [Range(1, 100)]
    /// @brief 최대 장탄수
    public int maxAmmo = 15;

    [Range(0, 100)]
    /// @brief 현재 잔탄의 수
    public int curAmmo = 15;

    //other compomponet
    Animator anim;
    NetworkPlayerController networkPlayerController;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;
    public AudioSource shotSource;
    public AudioSource reloadSource;

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

    /// @brief 총을 발사하고, 장전된 총알을 줄인다. 남은 탄환이 없다면 공격 대신 Reload()한다.
    /// @see AttackHandler.DoAttack()
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
        RPC_AudioPlay("shot");

        curAmmo--;
        StartCoroutine("Shot");
    }

    /// @brief bullet을 spawn하고 탄피를 배출한다.
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

    /// @brief 재장전한다.
    public override void Reload()
    {
        RPC_animatonSetTrigger("doReload");
        RPC_AudioPlay("reload");
        networkPlayerController.SetIsReload(true);
        StartCoroutine(ReloadOut(reloadTime));
    }
    /// @brief 재장전을 중단한다.
    public override void StopReload()
    {
        StopCoroutine("ReloadOut");
        networkPlayerController.SetIsReload(false);
    }

    /// brief time동안 기다리면 잔탄의 수를 최대 장탄수만큼 상승시킨다.
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

    /// @brief 애니메이션 동기화. server -> All
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_animatonSetTrigger(string action)
    {
        anim.SetTrigger(action);
    }
    /// @brief 탄피 배출하는 효과를 동기화. server -> All
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_bulletCase()
    {
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
    /// @brief 효과음 동기화. server -> All
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AudioPlay(string audioType)
    {
        switch (audioType)
        {
            case "shot":
                shotSource.Play();
                break;

            case "reload":
                reloadSource.Play();
                break;
        }

    }
}