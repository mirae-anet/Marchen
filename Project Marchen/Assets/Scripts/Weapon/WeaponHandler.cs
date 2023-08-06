using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 무기들이 기본적으로 상속받는 부모 클래스
public class WeaponHandler : NetworkBehaviour
{
    /// @brief 무기 타입의 종류
    public enum Type { Melee, Range };
    /// @brief 선택한 무기의 타입
    public Type type;
    /// @brief 공격
    /// @param aimDir 공격 방향
    public virtual void Attack(Vector3 aimDir){}
    /// @brief 재장전
    public virtual void Reload(){}
    /// @brief 재장전 중지
    public virtual void StopReload(){}
}
