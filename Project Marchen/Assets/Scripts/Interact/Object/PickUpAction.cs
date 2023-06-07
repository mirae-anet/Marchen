using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PickUpAction : InteractionHandler
{
    public enum Type { GreenBook, RedBook, BlueBattery, GreenBattary};
    [Header("설정")]
    public Type type;

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
            if (other.tag == "Player")
            {
                PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
                if(playerActionHandler != null)
                    playerActionHandler.action(transform);
    
                if(Spawner != null)
                {
                    Spawner.gameObject.SetActive(true);
                    Spawner.GetComponent<SpawnHandler>().SetTimer();
                }
                
                Runner.Despawn(Object);
            }
        }
    }
}
