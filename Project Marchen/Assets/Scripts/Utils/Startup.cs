using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 게임을 시작할 때 Resources/InstantiateOnLoad/ 디렉터리 아래에 위치한 모든 오브젝트를 로드함.
/// @details 현재는 GameManager만 있음. 시작할 때 connection token을 생성, 저장해야 하기 때문에 GameManager가 필요.
public class Startup  
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantiatePrefabs()
    {
        Debug.Log("-- Instantiating objects --");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach(GameObject prefab in prefabsToInstantiate)
        {
            Debug.Log($"Creating {prefab.name}");
            GameObject.Instantiate(prefab);
        }

        Debug.Log("-- Instantiating objects done --");

    }
}
