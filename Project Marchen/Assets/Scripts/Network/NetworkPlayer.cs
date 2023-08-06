using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

/// @brief 플레이어 기본 설정 및 동기화 관련 클래스
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local {get; set;}
    public Transform playerBody;
    public Transform WorldSpaceCanvas;

    /// @brief nickName 최대 16자
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName{get; set;} //최대 16자

    public bool FirstJoin = true;

    /// @brief Remote Client Token Hash
    /// @details need for Host migration
    [Networked] public int token {get; set;} 
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
        FirstJoin = false;
    }

    /// @brief 스폰시 실행함.
    /// @details 스폰시 필요한 카메라, 오디오 설정 및 UI 설정 등. 자세한 것은 코드의 주석을 참고.
    /// @see Spawner
    public override void Spawned()
    {
        bool Library = SceneManager.GetActiveScene().name == "Scene_2";
        
        //본인 
        if (Object.HasInputAuthority) //플레이어 본인
        {
            Local = this;
            LocalCameraHandler.Local = localCameraHandler;

            if (Library)
            {
                 //Sets the layer of the local players model
                //자신의 닉네임은 안 보이도록 레이어를 변경
                Utils.SetRenderLayerInChildren(playerBody, LayerMask.NameToLayer("LocalPlayerModel"));
                Utils.SetRenderLayerInChildren(WorldSpaceCanvas, LayerMask.NameToLayer("IgnoreCamera"));

                //Disable main camera
                if (Camera.main != null)
                    Camera.main.gameObject.SetActive(false);

                //Only 1 audio listener is allowed in the scene so enable loacl players audio listener
                AudioListener audioListener = GetComponentInChildren<AudioListener>(true); // true : inactive object도 대상에 포함.
                if (audioListener != null)
                    audioListener.enabled = true;

                //Enable the local camera
                // localCameraHandler.localCamera.enabled = true;
                localCameraHandler.localCameraEnable(true);

                //Enable UI for local player
                localUI.SetActive(true);

                //HP bar와 ItemCanvas는 비활성화해야함.

                //Detach camera if enabled
                localCameraHandler.transform.parent = null;

            }
            else
            {
                //Sets the layer of the local players model
                //자신의 닉네임은 안 보이도록 레이어를 변경
                Utils.SetRenderLayerInChildren(playerBody, LayerMask.NameToLayer("LocalPlayerModel"));
                Utils.SetRenderLayerInChildren(WorldSpaceCanvas, LayerMask.NameToLayer("IgnoreCamera"));

                //Disable main camera
                if (Camera.main != null)
                {
                    Camera.main.gameObject.SetActive(false);
                    Debug.Log("Deactivate main camera");
                }

                //Only 1 audio listener is allowed in the scene so enable local players audio listener
                AudioListener audioListener = localCameraHandler.GetComponentInChildren<AudioListener>(true); // true : inactive object도 대상에 포함.
                if(audioListener !=null)
                    audioListener.enabled = true;

                //Enable the local camera
                localCameraHandler.localCameraEnable(true);

                //Enable UI for local player
                localUI.SetActive(true);

                //Detach camera if enabled
                localCameraHandler.transform.parent = null;

            }

            RPC_SetNickName(GameManager.instance.playerNickName);

            Debug.Log("Spawned local player");
        }
        else //다른플레이어
        {
            if(Library)
            {
                //Disable the camera if we are not the local player
                // localCameraHandler.localCamera.enabled = false;
                localCameraHandler.localCameraEnable(false);

                //Disable UI in the PlayerUICanvas
                localUI.SetActive(false);

                Debug.Log("Spawned remote player");

            }
            else
            {
                //Disable the camera if we are not the local player
                // localCameraHandler.localCamera.enabled = false;
                localCameraHandler.localCameraEnable(false);

                //Disable UI in the PlayerUICanvas
                localUI.SetActive(false);

                Debug.Log("Spawned remote player");
            }
        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);

        transform.name = $"P_{Object.Id}";
        
    }

    /// @brief 플레이어 퇴장 시 실행.
    /// @details 퇴장 메시지 전송, connection token 관리. 자세한 것은 코드의 주석을 참고.
    public void PlayerLeft(PlayerRef player)
    {

        if(Object.HasStateAuthority)
        {
            //서버로 하여금 떠나간 플레이어에 해당하는 아바타만 "left" 메시지 발송
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if(playerLeftNetworkObject == Object)
                    //RPC message를 보내기 전에 아바타가 despawn되는 경우 메시지가 누락될 수 있어서.
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }
            // 떠나간 플레이어가 해당 아바타의 주인이라면 서버는 해당 아바타에 저장된 커넥션 토큰을 삭제한다. 그래야 나갔다가 다시 들어올 수 있음.
            if (player == Object.InputAuthority)
            {
                Spawner spawner = FindObjectOfType<Spawner>();
                if(spawner != null)
                {
                    foreach (KeyValuePair<int, NetworkPlayer> pair in spawner.mapTokenIDWithNetworkPlayer)
                    {
                        if (pair.Value == this)
                        {
                            spawner.mapTokenIDWithNetworkPlayer.Remove(pair.Key);
                            Runner.Despawn(Object);

                        }
                    }
                }
            }
        }
    }

    /// @brief playerNickNameTM은 static으로 만들 수 없어서 나눴다.
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnNickNameChanged value {changed.Behaviour.nickName}");
        changed.Behaviour.OnNickNameChanged();
    }
    /// @brief OnNickNameChanged()에서 호출
    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");
        playerNickNameTM.text = nickName.ToString();
    }

    /// @brief from client to server, set nickName. 참가 메시지 전송.
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


    /// @brief Get rid of the local camera if we get destroyed as a new one will be spawned with the new Network player   
    private void OnDestroy()
    {
        if(localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);
    }

    /// @brief scene 전환시 해야할 일(OnSceneLoaded) 추가.
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// @brief scene 이동 시 Spawned()를 다시 호출해서 해당 scene에 알맞은 세팅으로 변경.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"{Time.time} OnSceneLoaded: " + scene.name);

        if (scene.name != "Scene_1" && FirstJoin == false)
        {
            //Tell the host that we need to perform the spawned code manually
            if (Object != null && Object.HasStateAuthority && Object.HasInputAuthority)
            {
                Spawned();
            }

            if (Object != null && Object.HasStateAuthority)
            {
                CharacterRespawnHandler characterRespawnHandler = GetComponent<CharacterRespawnHandler>();
                if(characterRespawnHandler != null)
                {
                    characterRespawnHandler.ChangeSpawnPoint(new Vector3(0,0,0));
                    characterRespawnHandler.RequestRespawn();
                }
            }
        }
    }
}