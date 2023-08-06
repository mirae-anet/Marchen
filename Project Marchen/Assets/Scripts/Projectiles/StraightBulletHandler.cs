using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 일정시간동안 유지되는 움직이지 않는 투사체
public class StraightBulletHandler : NetworkBehaviour
{
    private bool isWait = false;
    /// @brief 적중 여부를 판정하는 기준점
    public Transform anchorPoint;
    /// @brief 상호작용하는 레이어들.
    public LayerMask collisionLayers;

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
    
    //Rocket info
    [Header("Bullet info")]
    /// @brief 투사체의 데이지
    public int damageAmount;
    /// @brief 판정 범위(반지름)
    public Vector3 boxSize;

    //other components
    NetworkObject networkObject;

    /// @brief 발사된 경우 투사체의 초기화.
    /// @details 발사한 주체의 정보, 투사체 수명 카운팅 시작
    public void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        this.firedByName = firedByName;;
        this.firedByPlayerRef = firedByPlayerRef;
        this.firedByNetworkObject = firedByNetworkObject;
        
        networkObject = GetComponent<NetworkObject>();

        Debug.Log("${Time.time} {firedByPlayerName} fire Bullet");
        StartCoroutine("WaitCO");
        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
    }
    /// @brief 생성 후 잠시(0.5초)동안은 데미지 입지않도록 조치
    IEnumerator WaitCO()
    {
        yield return new WaitForSeconds(0.5f);
        isWait = true;
    }

    /// @brief 수명이 지난 경우 Despawn. 적중 여부를 판정하고 적중시 hp 감소를 지시.
    /// @see HPHandler.OnTakeDamage(), EnemyHPHandler.OnTakeDamage()
    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            if(!isWait)
                return;
            
            //Check if the rocket has reached the end of its life
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(transform.forward), firedByPlayerRef, hits, collisionLayers, HitOptions.None);
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
        }
    }
}
