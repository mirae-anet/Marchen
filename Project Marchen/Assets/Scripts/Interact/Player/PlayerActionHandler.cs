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
        Debug.Log("Player");
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
