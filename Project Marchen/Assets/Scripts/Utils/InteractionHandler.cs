using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class InteractionHandler : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void RequestSpawn(NetworkBehaviour prefab, Vector3 position, Quaternion quaternion, GameObject spawnerSpawner)
    {
        if(Runner.IsServer)
            Runner.Spawn(prefab, position, quaternion);
        
        Destroy(spawnerSpawner);
    }
}