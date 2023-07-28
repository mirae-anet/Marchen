using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 상호작용과 관련된 스크립트의 부모 클래스. 상속 받아서 구현.
public class InteractionHandler : NetworkBehaviour
{
    /// @brief 해당 오브젝트와의 상호작용을 요청.
    /// @param other 상호작용을 요청하는 오브젝트의 transfrom.
    public virtual void action(Transform other){}

    /// @brief 특정한 prefab을 리스폰할 것을 요청.
    /// @param prefab 생성할 프리펩
    /// @param position 생성 위치
    /// @param quaternion 생성 방향
    /// @see SpawnerSpawner
    public void RequestSpawn(NetworkBehaviour prefab, Vector3 position, Quaternion quaternion)
    {
        if(Runner.IsServer)
            Runner.Spawn(prefab, position, quaternion);
    }
}
