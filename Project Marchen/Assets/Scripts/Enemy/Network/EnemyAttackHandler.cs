using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief Melee, Range, HeartQueen의 AttackHandler의 부모 클래스
public class EnemyAttackHandler : NetworkBehaviour
{
    /// @brief 플레이어가 공격 범위에 위치하는지 탐지.
    public virtual void Aiming(){}

    /// @brief 공격 캔슬. 
    public virtual void AttackCancel(){}
}
