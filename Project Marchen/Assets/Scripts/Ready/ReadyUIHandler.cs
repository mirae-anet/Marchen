using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// @brief 로비의 무기교체/던전 입장 UI에 관한 클래스
public class ReadyUIHandler : NetworkBehaviour
{
    /// @brief UI들
    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;
    public GameObject ReadyUiCanvas;
    public GameObject StartBtn;
    public GameObject LeftBtn;
    public GameObject RockImage;

    /// @brief 시작준비가 되어있는지
    bool isReady = false;

    /// @brief 시작전 카운트 다운 변수
    TickTimer countdownTickTimer = TickTimer.None;

    static byte countDown = 0;
    static byte previousCount = 0;

    /// @brief 카운트 다운 텍스트 초기화
    void Start()
    {
        countDownText.text = "";
    }

    /// @brief 게임 시작 조건
    void Update()
    {
        if (Object.HasStateAuthority)
        {
            // 카운트 다운이 완료시 게임 시작
            if (countdownTickTimer.Expired(Runner))
            {
                startGame();

                countdownTickTimer = TickTimer.None;
                if(Object.HasStateAuthority)
                    LeftUI();
            }// 카운트 다운 진행
            else if(countdownTickTimer.IsRunning)
            {
                countDown = (byte)countdownTickTimer.RemainingTime(Runner);
                if (countDown != previousCount)
                {
                    previousCount = countDown;
                    RPC_SetCountDown(countDown);
                }
            }
        }
    }

    /// @brief 던전으로 이동
    public void startGame()
    {
        //추가 입장이 불가능하도록 방 닫기
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);
        }
        //플레이어가 로비일 시
        if(SceneManager.GetActiveScene().name== "Scene_2" && gameObject.CompareTag("Alice"))
        {
            gameObject.tag = "Untagged";
            RPC_SetActiveReadyUI(false);
            isReady = false;
            buttonReadyText.text = "게임시작";
            PortalHandler potalHandler = FindObjectOfType<PortalHandler>();
            potalHandler.gameObject.GetComponent<Collider>().enabled = false;
            Runner.SetActiveScene("Scene_3");
            Debug.Log("앨리스");
        }
        else if(SceneManager.GetActiveScene().name == "Scene_2" && gameObject.CompareTag("Desert"))
        {
            gameObject.tag = "Untagged";
            RPC_SetActiveReadyUI(false);
            isReady = false;
            buttonReadyText.text = "게임시작";
            PortalHandler potalHandler = FindObjectOfType<PortalHandler>();
            potalHandler.gameObject.GetComponent<Collider>().enabled = false;
            Runner.SetActiveScene("DesertNet");
            Debug.Log("사막");
        }
        else
        {
            Runner.SessionInfo.IsOpen = true;
            Runner.SetActiveScene("Scene_2");
        }
    }

    /// @brief 해머로 무기변경
    public void OnChangeWeaponHammer()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(0);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// @brief 총으로 무기 변경
    public void OnChangeWeaponGun()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(1);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// @brief 게임시작 클릭시
    public void OnReady()
    {
        if (isReady)
            isReady = false;
        else isReady = true;

        if(Runner.IsServer)
        {
            //준비 완료시 카운트다운시작
            if (isReady)
            {
                countdownTickTimer = TickTimer.CreateFromSeconds(Runner, 5);
                buttonReadyText.text = "취소";
                LeftBtn.SetActive(false);
            }
            else //준비 취소
            {
                countdownTickTimer = TickTimer.None;
                countDown = 0;
                buttonReadyText.text = "게임시작";
                RPC_SetCountDown(countDown);
                LeftBtn.SetActive(true);
            }
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// @brief 카운트 다운 메시지 동기화
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCountDown(byte NewcountDown)
    {
        TextMeshProUGUI LocalcountDownText = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>().countDownText;
        countDown = NewcountDown;
        if (countDown == 0)
            LocalcountDownText.text = $"";
        else LocalcountDownText.text = $"Game start in {countDown}";
    }

    //@brief ReadyUI 창 종료
    public void LeftUI()
    { 
        // escHandler = FindObjectOfType<EscHandler>();
        EscHandler escHandler = LocalCameraHandler.Local.GetComponentInChildren<EscHandler>();
        if (escHandler.ActiveEsc())
        {
            RPC_SetActiveReadyUI(false);
            RPC_RotateCamera(true);
            return;
        }
        RPC_SetActiveReadyUI(false);
        RPC_MouseSet(false);
        RPC_RotateCamera(true);
    }

    //@brief ReadyUI창 활성화
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetActiveReadyUI(bool bol)
    {
        ReadyUIHandler readyUIHandler = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>(true);
        readyUIHandler.gameObject.SetActive(bol);
        if (GameManager.instance.ClearStage >= 1)
        {
            RockImage.SetActive(false);
        }
    }

    //@brief 마우스 활성화 및 동기화
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_MouseSet(bool set)
    {
        CharacterInputHandler inputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
        inputHandler.EnableinPut(!set);
        //마우스 활성화
        if (set)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
  
    }

    //@brief 카메라 고정 설정 및 동기화
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RotateCamera(bool enable)
    {
        Camera localCamera = FindLocalCamera();
        LocalCameraHandler camerahandler = localCamera.GetComponentInParent<LocalCameraHandler>();
        camerahandler.EnableRotationReady(enable);
    }

    //@brief UI 스타트/나가기 버튼 활성화
    public void SetActive()
    {
        StartBtn.SetActive(true);
        LeftBtn.SetActive(true);
    }

    //@brief 카메라 컴포넌트 찾기
    private Camera FindLocalCamera()
    {
        if(LocalCameraHandler.Local != null)
        {
            Camera localCamera = LocalCameraHandler.Local.GetComponentInChildren<Camera>();
            return localCamera;
        }
        else
        {
            return null;
        }
    }

}
