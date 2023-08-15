using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Fusion;

public class TrackerHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    [SerializeField]
    /// @brief 총알이 발사되는 기준점
    private Transform bulletPos;

    [SerializeField]
    /// @brief 발사할 총알
    private GameObject guidedBullet;

    /// @brief 타겟으로 지정할 수 있는 target의 최대 거리
    public float maxDistance = 500;
    /// @brief 조준할 수 있는 레이어
    public LayerMask layerMask;

    [Header("설정")]

    [Range(0f, 5f)]
    /// @brief 다음번 공격이 나가는데까지 걸리는 시간.
    public float delay = 0.1f;
    /// @brief 재장전하는데 필요한 시간
    public float reloadTime = 0.8f;

    [Range(1, 100)]
    /// @brief 최대 장탄수
    public int maxAmmo = 3;

    [Range(0, 100)]
    /// @brief 현재 잔탄의 수
    public int curAmmo = 3;

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
        type = Type.Tracker;
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
        Transform target;
        if(SeekTarget(aimForwardVector, out target)){
            yield return new WaitForSeconds(0.3f);
            Runner.Spawn(guidedBullet, bulletPos.position, Quaternion.LookRotation(aimForwardVector), Object.InputAuthority, (runner, spawnedBullet) =>
            {
                if(spawnedBullet.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
                    navMeshAgent.Warp(bulletPos.position);

                GuidedBulletHandler guidedBulletHandler = spawnedBullet.GetComponent<GuidedBulletHandler>();
                guidedBulletHandler.setTarget(target);
                guidedBulletHandler.Fire(Object.InputAuthority, networkObject, networkPlayer.nickName.ToString());
            });
            yield return new WaitForSeconds(delay);
        }
        networkPlayerController.SetIsAttack(false);
    }
    /// @brief 추적할 타겟을 찾는다.
    /// @details 타겟을 찾으면 true, 못 찾으면 false를 리턴. target으로 공격할 타겟의 Transform을 제공.
    /// @param target 찾은 타겟.
    /// @return bool
    private bool SeekTarget(Vector3 aimForwardVector, out Transform target){
        RaycastHit hit;
        if(Physics.SphereCast(bulletPos.position, 3.0f, aimForwardVector,out hit, maxDistance, layerMask)){
            target = hit.transform.root;
            return true;
        }else{
            target = null;
            return false;
        }
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
