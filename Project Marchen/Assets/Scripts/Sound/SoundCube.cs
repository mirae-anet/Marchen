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
    private AudioSource[] StopAudioSource;

    [Header("설정")]
    public bool isDestroy = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (StopAudioSource != null)
            {
                for (int i = 0; i < StopAudioSource.Length; i++)
                {
                    StopAudioSource[i].Stop();
                }
            }

            if (StartAudioSource != null)
                StartAudioSource.Play();

            if (isDestroy)
                Destroy(gameObject);
        }
    }
}
