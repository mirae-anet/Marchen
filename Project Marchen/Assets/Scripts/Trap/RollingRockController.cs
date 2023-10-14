using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingRockController : MonoBehaviour
{
    // 인스펙터
    [Header("설정")]
    [SerializeField]
    private float rollSpeed = 10.0f; // 회전 속도
    [SerializeField]
    private float forwardSpeed = 10.0f; // 굴러가는 속도

    private float currentRotation = 0.0f; // 현재 회전 각도
    private Transform rockMesh;

    void Awake()
    {
        rockMesh = GetComponent<Transform>();
    }

    void Update()
    {
        currentRotation += rollSpeed * Time.deltaTime;

        if (currentRotation >= 360.0f)
        {
            currentRotation -= 360.0f;
        }

        transform.Translate(Vector3.right * rollSpeed * Time.deltaTime);
        rockMesh.transform.Rotate(Vector3.forward, rollSpeed * Time.deltaTime);
    }
}