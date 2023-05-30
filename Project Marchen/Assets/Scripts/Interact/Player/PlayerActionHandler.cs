using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerActionHandler : InteractionHandler
{
    public LayerMask layerMask;
    public Transform BodyAnchor;
    Vector3 boxSize = new Vector3(2,2,2);

    //other component
    NetworkPlayerController networkPlayerController;

    void Start()
    {
        networkPlayerController = GetComponent<NetworkPlayerController>();
    }

    public override void action(Transform other)
    {
        if(!Object.HasStateAuthority)
            return;

        // Debug.Log("Player action");

        if(other.TryGetComponent<DespawnAction>(out DespawnAction despawnAction))
        {
            EnemyHPHandler[] enemyHPHandlers = FindObjectsOfType<EnemyHPHandler>();
            if(enemyHPHandlers.Length > 0)
            {
                for(int i = 0; i < enemyHPHandlers.Length; i++)
                {
                    if(enemyHPHandlers[i] != null)
                    {
                        enemyHPHandlers[i].OnTakeDamage("", Object, 255, transform.position); //MAX 즉사
                    }
                }
            }
        }

    }

    public void Interact()
    {
        Collider[] colliders  = Physics.OverlapBox(BodyAnchor.position, boxSize/2, Quaternion.LookRotation(transform.forward), layerMask);
        Debug.Log("Interact : " + colliders.Length);
        if(colliders.Length > 0)
        {
            for(int i = 0; i < colliders.Length; i++)
            {
                InteractionHandler interactionHandler = colliders[i].GetComponent<InteractionHandler>();

                if(interactionHandler != null)
                    interactionHandler.action(transform);
            }
        }
        // networkPlayerController.SetIsInteract(false);
    }

}
