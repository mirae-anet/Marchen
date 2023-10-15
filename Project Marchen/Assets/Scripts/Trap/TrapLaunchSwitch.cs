using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapLaunchSwitch : MonoBehaviour
{
    private bool isFalling = false;

    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject trapPrefab;
    [SerializeField]
    private Transform[] trapPorts;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isFalling)
        {
            TrapShot();
        }
    }

    private void TrapShot()
    {
        for (int i = 0; i < trapPorts.Length; i++)
        {
            Instantiate(trapPrefab, trapPorts[i].position, trapPorts[i].rotation);
        }

        isFalling = true;
    }
}