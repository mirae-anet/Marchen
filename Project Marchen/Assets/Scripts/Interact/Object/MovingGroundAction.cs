using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MovingGroundAction : InteractionHandler
{
    public float range;
    public float moveSpeed;
    public Vector3 moveDir;
    private bool move = false;

    [Networked]
    private Vector3 startPosition{get; set;}
    public bool skipSettingStartValues = false;
    private void Start() 
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
                startPosition = transform.position;    
        }
        else
        {
            if(Object.HasStateAuthority)
                transform.position = startPosition;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(!Object.HasStateAuthority)
            return;

        if(!move)
            return;
        
        transform.position += moveDir * Runner.DeltaTime * moveSpeed ;

        if(range <= Vector3.Distance(transform.position, startPosition))
        {
            Debug.Log("revert");
            moveDir.x *= -1;
            moveDir.y *= -1;
            moveDir.z *= -1;
        }

        move = false;
    }

    private void OnTriggerStay(Collider other) {
        if(!Object.HasStateAuthority)
            return;

        move = true;
    }

}
