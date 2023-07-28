using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
/// @brief 스토리 텍스트 출력
public class StoryTextAction : InteractionHandler
{
    /// @brief 출력할 텍스트. txt 파일.
    public TextAsset textAsset;

    /// @brief 최초 일회 일정한 범위에 들어서면 스토리 텍스트 출력을 요청할 수 있음.
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(!other.transform.root.GetComponent<NetworkObject>().HasInputAuthority)
            return;

        PrintStory();

        // this.GetComponent<SphereCollider>().radius = 3f;
        this.GetComponent<Collider>().enabled = false;
    }

    /// @brief f키를 눌러서 텍스트 출력을 요청할 수 있음.
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

    /// @brief 서버가 아닌 클라이언트들도 f를 눌러서 텍스트 출력.
    /// @details action과 RPC_action을 나눠 구현한 이유는 모든 상호작용은 서버에서 처리하기 때문이다. RPC_action은 서버가 아닌 클라이언트에서 상호작용(텍스트 출력)을 실행하기 위함이다.
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_action(NetworkObject obj)
    {
        if(obj.HasInputAuthority)
        {
            obj.GetComponent<PlayerActionHandler>().Interact();
        }
    }

    /// @brief 텍스트 출력
    /// @details local player의 story text ui를 찾아서 텍스트를 출력한다.
    private void PrintStory()
    {
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