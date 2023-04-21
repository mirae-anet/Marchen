using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;


public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local {get; set;}

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName{get; set;} //최대 16자
    void Start()
    {
        
    }
    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            Local = this;
            //Disable main camera
            Camera.main.gameObject.SetActive(false);

            //set nickname
            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickname"));

            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable the camera if we are not the local player
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            //Only 1 audio listener is allowednin the scene so disable remote players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            Debug.Log("Spawned remote player");
        }
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
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
    }
}
