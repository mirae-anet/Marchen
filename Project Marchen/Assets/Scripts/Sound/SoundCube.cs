using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCube : MonoBehaviour
{
    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private AudioSource StartAudioSource;
    [SerializeField]
    private AudioSource StopAudioSource;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (StopAudioSource != null)         
                StopAudioSource.Stop();

            if (StartAudioSource != null)
                StartAudioSource.Play();
        }
    }
}
