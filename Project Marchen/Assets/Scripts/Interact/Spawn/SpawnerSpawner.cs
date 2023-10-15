using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief Spawner를 스폰하는 스포너
/// @details SceneNetworkObject는 HostMigration시 사라져서 SceneObject인 SpawnerSpawner가 NetworkObject인 Spawner를 생성하도록 단계를 나눔.
/// @see SpawnerHandler
public class SpawnerSpawner : MonoBehaviour
{
    /// @brief 스폰할 스폰너 프리팹
    public NetworkBehaviour spawnerPF;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// @brief 일정 범위에 들어서면 동작.
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        InteractionHandler interactionHandler = other.transform.root.GetComponent<InteractionHandler>();
        interactionHandler.RequestSpawn(spawnerPF, transform.position, Quaternion.LookRotation(transform.forward));
        Destroy(transform.gameObject);
    }
}
