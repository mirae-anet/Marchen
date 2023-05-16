using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SpawnerSpawner : MonoBehaviour
{
    public NetworkBehaviour spawnerPF;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        InteractionHandler interactionHandler = other.transform.root.GetComponent<InteractionHandler>();
        interactionHandler.RequestSpawn(spawnerPF, transform.position, Quaternion.identity);
    
        Destroy(gameObject, 3);
    }
}
