using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TrapAction : InteractionHandler
{
    
    private Transform rotateObject;

    [Header("설정")]
    [SerializeField]
    private float trapSpeed = 10.0f; // 투사체 날아가는 속도

    [SerializeField]
    private float rollSpeed = 10.0f; // 회전 속도


    private void Start()
    {
        rotateObject = GetComponentsInChildren<Transform>()[1];
    }

    public override void FixedUpdateNetwork()
    {
        Moving();
        Rolling();
    }

    private void Moving()
    {
        transform.Translate(Vector3.forward * trapSpeed * Time.deltaTime);
    }

    private void Rolling()
    {
        rotateObject.Rotate(Vector3.right, rollSpeed * Time.deltaTime);
    }
}
