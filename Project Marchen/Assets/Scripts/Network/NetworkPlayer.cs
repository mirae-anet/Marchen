using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;


public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local {get; set;}
    public Transform playerModel;

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName{get; set;} //최대 16자

    // Remote Client Token Hash
    [Networked] public int token {get; set;} //need for Host migration
    bool isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;

    //other components
    NetworkInGameMessages networkInGameMessages;

    private void Awake() 
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();    
    }
    void Start()
    {
        
    }
    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            Local = this;

            //Sets the layer of the local players model
            //자신의 닉네임은 안 보이도록 레이어를 변경
            Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

            //Disable main camera
            Camera.main.gameObject.SetActive(false);

            //set nickname
            //key(PlayerNickname)에 해당하는 문자열을 preference에서 가져온다.
            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickname"));

            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable the camera if we are not the local player
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            //Only 1 audio listener is allowed in the scene so disable remote players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            //Disable UI in the PlayerUICanvas
            localUI.SetActive(false);

            Debug.Log("Spawned remote player");
        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(Object.HasStateAuthority)
        {
            //서버로 하여금 떠나간 플레이어에 해당하는 아바타만 "left" 메시지 발송
            if(Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if(playerLeftNetworkObject == Object)
                    //RPC message를 보내기 전에 아바타가 despawn되는 경우 메시지가 누락될 수 있어서.
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }
        }

        if(player == Object.InputAuthority)
            Runner.Despawn(Object);
    }
    //playerNickNameTM은 static으로 만들 수 없어서 나눴다.
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.nickName}");
        changed.Behaviour.OnNickNameChanged();
    }
    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");
        playerNickNameTM.text = nickName.ToString();
    }
    //from client to server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if(!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "joined");
            isPublicJoinMessageSent = true;
        }
    }
}
