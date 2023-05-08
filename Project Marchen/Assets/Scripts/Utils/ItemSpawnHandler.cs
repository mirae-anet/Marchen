using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ItemSpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private HeartHandler itemPrefab;
    [SerializeField]
    private Transform anchorPoint;
    private bool spawnAble = true;

    TickTimer respawnDelay = TickTimer.None;

    void Start()
    {
        if(TryGetBehaviour<NetworkRunner>(out NetworkRunner networkRunner))
        {
            if(networkRunner.IsServer)
                SpawnItem(networkRunner);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!spawnAble)
            return;

        if (other.tag != "Player")
            return;

        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(!networkRunner.IsServer)
            return;

        if(respawnDelay.ExpiredOrNotRunning(networkRunner))
            SpawnItem(networkRunner);
    }
    private void SpawnItem(NetworkRunner networkRunner)
    {
        HeartHandler spawnedItem = networkRunner.Spawn(itemPrefab, anchorPoint.position, Quaternion.identity);
        spawnedItem.itemSpawner = Object;
        Debug.Log($"spawn item");
        spawnAble = false;
    }
    public void SetTimer()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner.IsServer)
            respawnDelay = TickTimer.CreateFromSeconds(networkRunner, delayTime);
        spawnAble = true;
    }
}
