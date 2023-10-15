using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMove : MonoBehaviour
{
    private Transform rotateObject;

    [Header("설정")]
    [SerializeField]
    private float trapSpeed = 10.0f; // 투사체 날아가는 속도

    [Space(10)]
    [SerializeField]
    private bool isRolling = false;
    [SerializeField]
    private float rollSpeed = 10.0f; // 회전 속도

    private void Start()
    {
        if (!isRolling)
            return;

        rotateObject = GetComponentsInChildren<Transform>()[1];
    }

    private void FixedUpdate()
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
        if (!isRolling)
            return;

        rotateObject.Rotate(Vector3.right, rollSpeed * Time.deltaTime);
    }
}
