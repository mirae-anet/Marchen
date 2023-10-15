using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefabToSpawn; // 생성할 프리팹
    [SerializeField]
    private Transform spawnPoint; // 프리팹이 생성될 위치
    [SerializeField]
    private float spawnInterval = 10.0f; // 생성 간격 (초)

    void Start()
    {
        // 시작하자마자 코루틴을 시작하여 일정 간격으로 프리팹을 생성
        StartCoroutine(SpawnPrefabRepeatedly());
    }

    IEnumerator SpawnPrefabRepeatedly()
    {
        while (true)
        {
            // 프리팹을 생성하고 지정된 위치 배치
            Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

            // 일정 시간만큼 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
