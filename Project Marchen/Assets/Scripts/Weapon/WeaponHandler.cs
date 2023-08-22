using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    /// @brief 무기 타입의 종류
    public enum Type { Melee, Range, Tracker};
    /// @brief 선택한 무기의 타입
    public Type type;
    public virtual void Attack(Vector3 aimDir){}
    public virtual void Reload(){}
    public virtual void StopReload(){}
}
