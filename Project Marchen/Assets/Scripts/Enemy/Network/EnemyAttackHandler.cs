using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

//Melee와 Range AttackHandler의 부모
public class EnemyAttackHandler : NetworkBehaviour
{
    public virtual void Aiming(){}

    public virtual void AttackCancel(){}
}
