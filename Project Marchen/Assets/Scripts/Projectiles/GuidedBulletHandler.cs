using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @brief 목표를 추적하는 투사체
public class GuidedBulletHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    /// @brief 적중 여부를 판정하는 기준점
    public Transform checkForImpactPoint;
    /// @brief 상호작용하는 레이어들.
    public LayerMask collisionLayers;

    //Thrown by info
    /// @brief 발사한 투사체의 playerRef. 몬스터의 탄환이면 서버의 ref. 
    PlayerRef firedByPlayerRef;
    /// @brief 발사한 주체의 이름.
    string firedByName;
    /// @brief 발사한 주체의 NetworkObject
    NetworkObject firedByNetworkObject;

    /// @brief Hit info
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    
    /// @brief 발사된 투사체의 수명을 카운트하기 위한 타이머.
    TickTimer maxLiveDurationTickTimer = TickTimer.None;
    
    [Header("Bullet info")]
    /// @brief 투사체의 데이지
    public int damageAmount;
    /// @brief 데미지를 주는 범위 (폭팔 범위)
    public float radius = 4;
    /// @brief 추적할 타겟
    private Transform target;

    //other components
    NavMeshAgent nav;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    /// @brief 발사된 경우 투사체의 초기화.
    /// @details 발사한 주체의 정보, 투사체 수명 카운팅 시작
    public void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 6);
    }

    /// @brief 프레임마다 이동, 적중 여부를 판정, 수명이 지난 경우 Despawn.
    public override void FixedUpdateNetwork()
    {
        Move();

        if(Object.HasStateAuthority)
        {
            //Check if the rocket has reached the end of its life
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
                return;
            }

            CheckForImpactPoint();
        }
    }

    /// @brief 이동(타겟을 추적)
    private void Move()
    {
        if (target == null)
            return;

        nav.SetDestination(target.position);
    }

    /// @brief 적중 여부를 판정하고 적중시 hp 감소를 지시.
    /// @see HPHandler.OnTakeDamage(), EnemyHPHandler.OnTakeDamage()
    private void CheckForImpactPoint(){
        int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 1.25f, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);

        if(hitCount > 0)
        {
            //Now we need to figure out of anything was within the blast radius
            hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, radius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

            //Deal damage to anything within the hit radius
            for(int i = 0; i < hitCount; i++)
            {
                if(hits[i].Hitbox.Root.TryGetComponent<HPHandler>(out HPHandler hpHandler))
                {
                    if(firedByNetworkObject != null)
                        hpHandler.OnTakeDamage(firedByName, damageAmount, transform.position);
                }
                if(hits[i].Hitbox.Root.transform.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
                {
                    if(firedByNetworkObject != null)
                        enemyHPHandler.OnTakeDamage(firedByName, firedByNetworkObject, damageAmount, transform.position);
                }
            }

            Runner.Despawn(Object);
        }
    }

    /// @brief 타겟을 설정
    public void setTarget(Transform vec)
    {
        target = vec;
    }
}
