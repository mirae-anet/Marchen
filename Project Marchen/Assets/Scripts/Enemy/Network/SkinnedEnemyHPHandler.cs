using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SkinnedEnemyHPHandler : EnemyHPHandler
{
    private SkinnedMeshRenderer[] skinnedMeshs;
    
    protected override void Awake()
    {
        base.Awake();
        skinnedMeshs = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
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
