using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HeartHandler : NetworkBehaviour
{
    [Header("설정")]
    [Range(1f, 100f)]
    public byte value = 20;
    // public int value = 20;
    // public enum Type {Heart};
    // public Type type;

    //other component
    NetworkObject networkObject;
    public NetworkObject itemSpawner;

    void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();

        if(networkRunner.IsServer)
        {
            // Debug.Log($"Heart OnTriggerEnter : {other.tag}");
    
            if (other.tag == "Player")
            {
                HPHandler hpHandler = other.transform.root.GetComponent<HPHandler>();
                hpHandler.OnHeal(value);
    
                if(itemSpawner != null)
                {
                    itemSpawner.gameObject.SetActive(true);
                    itemSpawner.GetComponent<ItemSpawnHandler>().SetTimer();  
                }
                
                Runner.Despawn(networkObject);
            }
        }
    }
    private void OnDestroy()
    {
    }
}
