using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 아이템 회전
public class ItemRotate : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }
}