using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StoryTextAction : InteractionHandler
{
    private bool first = true;
    private void OnTriggerEnter(Collider other)
    {
        if(!first)
            return;

        if (other.tag != "Player")
            return;
        
        PrintStory();

        // this.GetComponent<SphereCollider>().radius = 3f;
        this.GetComponent<SphereCollider>().enabled = false;
        first = false;
    }

    public override void action(Transform other)
    {
        Debug.Log("OnAction");

        if(other == null)
            return;

        if(other.GetComponent<NetworkObject>().HasInputAuthority)
        {
            PrintStory();
        }
        else if(Runner != null && Runner.IsServer)
        {
            RPC_action(other.GetComponent<NetworkObject>());
        }
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_action(NetworkObject obj)
    {
        if(obj.HasInputAuthority)
        {
            obj.GetComponent<PlayerActionHandler>().Interact();
        }
    }
    private void PrintStory()
    {
        // 자신의 StoryTextUI에 스토리를 설명하는 Text를 출력 
        Debug.Log("OnPrintStroy");
    }
}