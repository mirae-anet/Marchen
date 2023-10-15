using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatTrapLauncher : MonoBehaviour
{
    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject trapPrefab;

    [Header("설정")]
    [SerializeField]
    private float launchCool = 10.0f; // 생성 간격 (초)

    void Start()
    {
        StartCoroutine(RepeatLaunch());
    }

    IEnumerator RepeatLaunch()
    {
        while (true)
        {
            // 프리팹을 생성하고 지정된 위치 배치 Quaternion.identity
            Instantiate(trapPrefab, transform.position, transform.rotation);

            // 일정 시간만큼 대기
            yield return new WaitForSeconds(launchCool);
        }
    }
}
