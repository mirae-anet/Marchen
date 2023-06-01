using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public virtual void Attack(Vector3 aimDir){}
    public virtual void Reload(){}
    public virtual void StopReload(){}
}
