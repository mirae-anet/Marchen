using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCube : MonoBehaviour
{
    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private AudioSource audioSource;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            audioSource.Play();
        }
    }
}
