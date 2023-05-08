using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemySpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private EnemyHPHandler enemyPrefab;
    [SerializeField]
    private Transform anchorPoint;
    private bool spawnAble = true;

    TickTimer respawnDelay = TickTimer.None;

    void Start()
    {
        if(TryGetBehaviour<NetworkRunner>(out NetworkRunner networkRunner))
        {
            if(networkRunner.IsServer)
                SpawnEnemy(networkRunner);
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
            SpawnEnemy(networkRunner);
    }
    private void SpawnEnemy(NetworkRunner networkRunner)
    {
        EnemyHPHandler spawnedEnemy = networkRunner.Spawn(enemyPrefab, anchorPoint.position, Quaternion.identity);
        spawnedEnemy.enemySpawner = Object;
        Debug.Log($"spawn item");
        spawnAble = false;
    }
    public void SetTimer()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null && networkRunner.IsServer)
            respawnDelay = TickTimer.CreateFromSeconds(networkRunner, delayTime);
        spawnAble = true;
    }
}
