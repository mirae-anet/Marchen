using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockRotate : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.right * 100 * Time.deltaTime);
    }
}
