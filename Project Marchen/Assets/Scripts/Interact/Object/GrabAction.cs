using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief RigidBody를 잡아서 옮기는 기능 추가.
public class GrabAction : InteractionHandler
{
    /// @brief 잡고있는지 여부.
    private bool isGrab = false;
    /// @brief 해당 오브젝트의 rigidbody
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
            
            Utils.SetRenderLayerInChildren(this.transform,LayerMask.NameToLayer("UI"));
            this.transform.parent = other;

            isGrab = true;
        }
        else
        {
            this.transform.parent = null;
            Utils.SetRenderLayerInChildren(this.transform,LayerMask.NameToLayer("Ground"));

            rigid.isKinematic = false;

            if(networkPlayerController != null)
                networkPlayerController.SetIsInteract(false);

            isGrab = false;
        }
    }
}
