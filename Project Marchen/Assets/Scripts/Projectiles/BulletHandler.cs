using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 원거리 투사체
public class BulletHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    /// @brief 적중시 시각적인 효과
    GameObject explosionParticleSystemPrefab;

    [Header("Collsion")]
    [SerializeField]
    /// @brief 적중 여부를 판정하는 기준점
    private Transform checkForImpactPoint;

    /// @brief 상호작용하는 레이어들.
    [SerializeField]
    private LayerMask collisionLayers;

    /// @brief 폭팔 시 효과를 적용하는 투사체인 경우 true로 설정할 것.
    public bool isExplosion;

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
    [SerializeField]
    int damageAmount;
    /// @brief 판정 범위(반지름)
    [SerializeField]
    float checkRadius = 0.5f;
    /// @brief 데미지를 주는 범위 (폭팔 범위)
    [SerializeField]
    float damageRadius = 4;
    /// @brief 투사체 속도
    [SerializeField]
    int bulletSpeed;

    //other components
    NetworkObject networkObject;

    /// @brief 발사된 경우 투사체의 초기화.
    /// @details 발사한 주체의 정보, 투사체 수명 카운팅 시작
    public virtual void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;
        
        networkObject = GetComponent<NetworkObject>();

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    /// @brief 프레임마다 이동, 적중 여부를 판정, 수명이 지난 경우 Despawn.
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            Move();

            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            CheckForImpactPoint();
        }
    }

    /// @brief 이동
    protected virtual void Move()
    {
        transform.position += transform.forward * Runner.DeltaTime * bulletSpeed;
    }

    /// @brief 적중 여부를 판정하고 적중시 hp 감소를 지시.
    /// @see HPHandler.OnTakeDamage(), EnemyHPHandler.OnTakeDamage()
    protected virtual void CheckForImpactPoint()
    {
        int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, checkRadius, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);

        if(hitCount > 0)
        {
            //Now we need to figure out of anything was within the blast radius
            hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, damageRadius, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

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

            Runner.Despawn(networkObject);
        }
    }

    /// @brief When despawning the object we want to create a visual explosion
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(isExplosion)
            Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.transform.position, Quaternion.identity);
    }
}
