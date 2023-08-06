using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 기타 유틸리티 함수
public static class Utils
{
    /// @brief 0을 기준으로 +5 ~ -5 범위의 랜덤한 스폰 위치를 획득
    public static Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5));
    }
    /// @brief spawnPoint를 기준으로 +range ~ -range 범위의 랜덤한 스폰 위치를 획득
    /// @param spawnPoint 스폰되기 원하는 대략적인 위치값.
    /// @param range spawnPoint로부터 떨어질 수 있는 최대 거리(편차)
    /// @return Vector3 새로운 스폰 위치
    public static Vector3 GetRandomSpawnPoint(Vector3 spawnPoint, float range)
    {
        return spawnPoint + new Vector3(Random.Range(-range, range), 5, Random.Range(-range, range));
    }

    /// @brief transform 아래 자식 레이어의 layer를 layerNumber로 변경한다.
    /// @details tag가 IgnoreLayerChange라면 무시한다.
    /// @param transform 해당 transform부터 모든 자식들의 layer가 대상이 된다.
    /// @param layerNumber 변경하고자 하는 layer의 번호.
    /// @see GrabAction, NetworkPlayer.Spawned()
    public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
    {
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            if (trans.CompareTag("IgnoreLayerChange"))
                continue;

            trans.gameObject.layer = layerNumber;
        }
    }
}
