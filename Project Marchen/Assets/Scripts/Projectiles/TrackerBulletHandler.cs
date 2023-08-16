using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

/// @brief 목표를 추적하는 투사체
public class TrackerBulletHandler : BulletHandler
{
    /// @brief trackerBullet이 한번에 타겟을 향해 회전할 수 있는 정도. (0~1f 사이)
    public float rotationSpeed = 0.1f;

    /// @brief 추적할 타겟
    private Transform target;

    /// @brief target을 향해서 추적
    protected override void Move()
    {
        base.Move();
        if(target == null) return;

        // target을 향하는 벡터를 구함.
        Vector3 targetDirection = (target.position - transform.position).normalized;

        // 목표 방향으로의 회전을 나타내는 Quaternion을 얻습니다.
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // 현재 회전에서 목표 회전으로 점진적으로 회전합니다.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// @brief 타겟을 설정
    public void setTarget(Transform vec)
    {
        target = vec;
    }

}
