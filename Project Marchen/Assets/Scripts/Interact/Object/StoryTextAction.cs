using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class StoryTextAction : InteractionHandler
{
    public TextAsset textAsset;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(!other.transform.root.GetComponent<NetworkObject>().HasInputAuthority)
            return;

        PrintStory();

        // this.GetComponent<SphereCollider>().radius = 3f;
        this.GetComponent<SphereCollider>().enabled = false;
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
        //local player의 story text ui를 찾는다.
        if(LocalCameraHandler.Local != null)
        {
            StoryTextUIHandler storyTextUIHandler = LocalCameraHandler.Local.GetComponentInChildren<StoryTextUIHandler>(true);
            if(storyTextUIHandler != null)
            {
                Debug.Log("OnPrintStory");
                storyTextUIHandler.gameObject.SetActive(true);
                storyTextUIHandler.StartStory(textAsset);
            }
        }
    }

}