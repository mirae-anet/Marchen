using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 배경음악 관련 클래스
public class SoundCube : MonoBehaviour
{
    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    /// @brief 재생 시작 음원
    private AudioSource StartAudioSource;
    [SerializeField]
    /// @brief 재생 중지 음원 배열
    private AudioSource[] StopAudioSource;

    [Header("설정")]
    /// @brief 충돌 시 오브젝트 파괴 여부
    public bool isDestroy = true;

    /// @brief 트리거 작동 시 음원 On/Off
    /// @details 재생 중지 음원이 있으면 중지, 재생 시작 음원이 있으면 재생. 파괴 여부 체크 시 사운드 큐브 오브젝트 파괴.
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
