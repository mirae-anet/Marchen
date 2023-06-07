using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuidedBullet : BulletMain
{
    private Transform target;
    private NavMeshAgent nav;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        move();
    }

    void move()
    {
        if (target == null)
            return;

        nav.SetDestination(target.position);
    }

    public void setTarget(Transform vec)
    {
        target = vec;
    }
}