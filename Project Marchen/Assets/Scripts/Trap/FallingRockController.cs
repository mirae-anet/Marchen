using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRockController : MonoBehaviour
{
    // 인스펙터
    [Header("설정")]
    [SerializeField]
    private GameObject FallObject; // 떨어지는 큐브
    [SerializeField]
    private float fallSpeed = 10.0f; // 큐브가 떨어지는 속도

    private bool isFalling = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isFalling)
        {
            FallCube();
        }
    }

    void FallCube()
    {
        isFalling = true;
    }

    void Update()
    {
        if (isFalling)
        {
            FallObject.transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
    }
}