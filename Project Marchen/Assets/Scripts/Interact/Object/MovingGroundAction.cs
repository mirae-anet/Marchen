using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 움직이는 그라운드 구현
public class MovingGroundAction : InteractionHandler
{
    /// @brief 한 방향으로 이동할 수 있는 최대 거리.
    public float range;
    /// @brief 이동 속도.
    public float moveSpeed;
    /// @brief 이동하는 축.
    /// @details x,y,z 축을 따라서 이동하고 싶으면 각각의 값에 1을 넣으면 됨.
    public Vector3 moveDir;
    private bool move = false;

    /// @brief 왕복 운동의 기준점
    /// @details hostmigration 전후를 동일하게 유지하기 위해서 networked함.
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
    /// @brief 일정한 범위(collider) 안에 위치한 경우에만 동작.
    private void OnTriggerStay(Collider other) {
        if(!Object.HasStateAuthority)
            return;

        move = true;
    }

}
