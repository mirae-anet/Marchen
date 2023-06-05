using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5));
    }
    public static Vector3 GetRandomSpawnPoint(Vector3 spawnPoint)
    {
        return spawnPoint + new Vector3(Random.Range(-5, 5), 5, Random.Range(5, 5));
    }

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
