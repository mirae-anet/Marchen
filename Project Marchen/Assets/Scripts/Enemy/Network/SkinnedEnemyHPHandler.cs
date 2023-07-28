using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief SkinnedMesh를 사용하는 에너미 HP 관련 클래스.
public class SkinnedEnemyHPHandler : EnemyHPHandler
{
    protected SkinnedMeshRenderer[] skinnedMeshs;
    
    protected override void Awake()
    {
        base.Awake();
        skinnedMeshs = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
    /// @brief 피격시 동작 
    /// @details 모든 컴퓨터에서 동작함. SkinnedMesh의 색상 변화, isDamage 변수값 변경.
    protected override IEnumerator OnHitCO()
    {
        // 피격시 효과
        isDamage = true;
        anim.SetBool("isWalk", false);

        foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshs)
            skinnedMesh.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        //아바타 정상화
        isDamage = false;

        foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshs)
            skinnedMesh.material.color = Color.white;
    }
}
