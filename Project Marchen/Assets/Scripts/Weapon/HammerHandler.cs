using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 해머의 기능을 구현하는 클래스
public class HammerHandler : WeaponHandler
{
    [Header("오브젝트 연결")]
    /// @brief 피격 판정의 기준점
    public Transform anchorPoint;
    /// @brief 피격 판정의 범위(박스)
    private Vector3 boxSize = new Vector3(2f, 2f, 2f);

    [SerializeField]
    private TrailRenderer trailEffect;

    [Header("설정")]
    [Range(1f, 100f)]
    /// @brief 데미지
    public int damageAmount = 25;
    [Range(0f, 5f)]
    public float delay = 0.6f;
    public AudioSource swingSource;

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
        type = Type.Melee;
    }

    /// @brief 공격을 수행한다. 아바타의 애니메이션을 통해서 해머를 스윙한다.
    /// @see AttackHandler.DoAttack()
    public override void Attack(Vector3 aimDir)
    {
        RPC_animatonSetTrigger("doSwing");
        RPC_AudioPlay("swing");
        StartCoroutine("Swing");
    }

    /// @brief 스윙 중에 피격 판정을 수행한다.
    /// @see EnemyHPHandler.OnTakeDamage()
    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.2f);
        RPC_SetTrailEffect(true);

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        float endTime = Time.time + 0.35f;
        while (Time.time < endTime)
        {
            int hitCount = Runner.LagCompensation.OverlapBox(anchorPoint.position, boxSize/2, Quaternion.LookRotation(transform.forward), Object.InputAuthority, hits, LayerMask.GetMask("EnemyHitBox"));
            if(hitCount > 0)
            {
                for(int i = 0; i < hitCount; i++)
                {
                    EnemyHPHandler enemyHPHandler = hits[i].Hitbox.Root.GetComponent<EnemyHPHandler>();

                    if(enemyHPHandler != null)
                        enemyHPHandler.OnTakeDamage(networkPlayer.nickName.ToString(), networkObject, damageAmount, transform.position);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
        RPC_SetTrailEffect(false);

        networkPlayerController.SetIsAttack(false);
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

    /// @brief 해머의 trail effect를 동기화. server -> All
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetTrailEffect(bool bol)
    {
        trailEffect.enabled = bol;
    }

    /// @brief 효과음 동기화. server -> All
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AudioPlay(string audioType)
    {
        swingSource.Play();
    }
}
