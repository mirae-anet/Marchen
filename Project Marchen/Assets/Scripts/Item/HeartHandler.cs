using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HeartHandler : NetworkBehaviour
{
    [Header("설정")]
    [Range(1f, 100f)]
    public int value = 20;
    // public int value = 20;
    // public enum Type {Heart};
    // public Type type;

    //other component
    public NetworkObject Spawner;

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Object != null && Object.HasStateAuthority)
        {
            // Debug.Log($"Heart OnTriggerEnter : {other.tag}");
    
            if (other.tag == "Player")
            {
                HPHandler hpHandler = other.transform.root.GetComponent<HPHandler>();
                if(hpHandler != null)
                    hpHandler.OnHeal(value);
    
                if(Spawner != null)
                {
                    Spawner.gameObject.SetActive(true);
                    Spawner.GetComponent<SpawnHandler>().SetTimer();  
                }
                
                Runner.Despawn(Object);
            }
        }
    }
    private void OnDestroy()
    {
    }
}
