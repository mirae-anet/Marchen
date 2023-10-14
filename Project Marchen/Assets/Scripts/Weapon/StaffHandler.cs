using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using Fusion;

public class StaffHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    [SerializeField]
    /// @brief 총알이 발사되는 기준점
    private Transform bulletPos;
    /// @brief 발사 이팩트가 발생하는 기준점
    public Transform FlamePos;
    /// @brief 발사시 시각적인 효과
    public GameObject FlameParticleSystemPrefab;

    [SerializeField]
    /// @brief 발사할 총알
    private GameObject trackerBullet;

    /// @brief 타겟으로 지정할 수 있는 target의 최대 거리
    public float maxDistance = 500;
    /// @brief 조준할 수 있는 레이어
    public LayerMask layerMask;

    [Header("설정")]

    [Range(0f, 5f)]
    /// @brief 다음번 공격이 나가는데까지 걸리는 시간.
    public float delay = 1.1f;

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
        type = Type.Staff;
    }

    /// @brief 총을 발사하고, 장전된 총알을 줄인다. 남은 탄환이 없다면 공격 대신 Reload()한다.
    /// @see AttackHandler.DoAttack()
    public override void Attack(Vector3 aimDir)
    {
        networkPlayerController.RPC_LookForward(aimDir);
        RPC_animatonSetTrigger("doShot");
        StartCoroutine("Shot");
    }

    /// @brief bullet을 spawn하고 탄피를 배출한다.
    IEnumerator Shot()
    {
        Vector3 aimForwardVector = networkPlayerController.aimForwardVector;
        Vector3 launchVector = new Vector3(aimForwardVector.x * 0.8f, 0.8f,aimForwardVector.z * 0.8f);
        Transform target;
        if(SeekTarget(aimForwardVector, out target)){
            yield return new WaitForSeconds(0.3f);
            Runner.Spawn(trackerBullet, bulletPos.position, Quaternion.LookRotation(launchVector), Object.InputAuthority, (runner, spawnedBullet) =>
            {
                TrackerBulletHandler trackerBulletHandler = spawnedBullet.GetComponent<TrackerBulletHandler>();
                trackerBulletHandler.setTarget(target);
                trackerBulletHandler.Fire(Object.InputAuthority, networkObject, networkPlayer.nickName.ToString());
            });
            RPC_AudioPlay("shot");
            RPC_MakeFlame();
            yield return new WaitForSeconds(delay);
        }
        networkPlayerController.SetIsAttack(false);
    }
    /// @brief 추적할 타겟을 찾는다.
    /// @details 타겟을 찾으면 true, 못 찾으면 false를 리턴. target으로 공격할 타겟의 Transform을 제공.
    /// @param target 찾은 타겟.
    /// @return bool
    private bool SeekTarget(Vector3 aimForwardVector, out Transform target){
        Debug.Log("start to find target");
        RaycastHit[] hits = Physics.SphereCastAll(bulletPos.position, 10.0f, aimForwardVector, maxDistance, layerMask);
        Debug.Log("hits : " + hits.Length);
        if(hits.Length > 0){
            //Debug.DrawRay(bulletPos.position, aimForwardVector*50.0f, Color.green, 5.0f);
            target = hits[0].transform.root;
            //Debug.Log("finding target sucess");
            return true;
        }else{
            //Debug.DrawRay(bulletPos.position, aimForwardVector*50.0f, Color.red, 5.0f);
            target = null;
            //Debug.Log("finding target fail");
            return false;
        }
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
        }

    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MakeFlame()
    {
        //여기
        Instantiate(FlameParticleSystemPrefab, FlamePos.transform.position, Quaternion.LookRotation(transform.forward));
    }
}
