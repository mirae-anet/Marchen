using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAction : InteractionHandler
{
    private bool isGrab = false;
    private Rigidbody rigid;

    private void Awake() {
        rigid = GetComponentInParent<Rigidbody>();
    }

    public override void action(Transform other)
    {
        Debug.Log("OnAction");

        if(other == null)
            return;

        NetworkPlayerController networkPlayerController = other.GetComponent<NetworkPlayerController>();

        if(!isGrab)
        {
            rigid.velocity = Vector3.zero;
            rigid.isKinematic = true;

            if(networkPlayerController != null)
                networkPlayerController.SetIsInteract(true);
            
            this.transform.parent = other;

            isGrab = true;
        }
        else
        {
            this.transform.parent = null;

            rigid.isKinematic = false;

            if(networkPlayerController != null)
                networkPlayerController.SetIsInteract(false);

            isGrab = false;
        }
    }
}
