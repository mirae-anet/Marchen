using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;
    public GameObject ReadyUiCanvas;
    public GameObject StartBtn;
    public GameObject LeftBtn;

    bool isReady = false;

    //countdown
    TickTimer countdownTickTimer = TickTimer.None;

    static byte countDown = 0;
    static byte previousCount = 0;
    void Start()
    {
        countDownText.text = "";
    }

    void Update()
    {
        if (Object.HasStateAuthority)
        {
            if (countdownTickTimer.Expired(Runner))
            {
                startGame();

                countdownTickTimer = TickTimer.None;
                if(Object.HasStateAuthority)
                    LeftUI();
            }
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

    public void startGame()
    {
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);
        }//시작
        if(SceneManager.GetActiveScene().name== "Scene_2")
        {
            RPC_SetActiveReadyUI(false);
            isReady = false;
            buttonReadyText.text = "게임시작";
            Runner.SetActiveScene("Scene_3");
        }
        else
        {
            Runner.SessionInfo.IsOpen = true;
            Runner.SetActiveScene("Scene_2");
        }
    }
    public void OnChangeWeaponHammer()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(0);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OnChangeWeaponGun()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(1);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnReady()
    {
        if (isReady)
            isReady = false;
        else isReady = true;

        if(Runner.IsServer)
        {
            //startGame();
            if (isReady)
            {
                countdownTickTimer = TickTimer.CreateFromSeconds(Runner, 5);
                buttonReadyText.text = "취소";
            }
            else
            {
                countdownTickTimer = TickTimer.None;
                countDown = 0;
                buttonReadyText.text = "게임시작";
                countDown = 0;
                RPC_SetCountDown(countDown);
            }
        }
        EventSystem.current.SetSelectedGameObject(null);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCountDown(byte NewcountDown)
    {
        TextMeshProUGUI LocalcountDownText = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>().countDownText;
        countDown = NewcountDown;
        if (countDown == 0)
            LocalcountDownText.text = $"";
        else LocalcountDownText.text = $"Game start in {countDown}";
    }

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetActiveReadyUI(bool bol)
    {
        ReadyUIHandler readyUIHandler = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>(true);
        readyUIHandler.gameObject.SetActive(bol);
    }

    //마우스잠금
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RotateCamera(bool enable)
    {
        Camera localCamera = FindLocalCamera();
        LocalCameraHandler camerahandler = localCamera.GetComponentInParent<LocalCameraHandler>();
        camerahandler.EnableRotationReady(enable);
    }
    public void SetActive()
    {
        StartBtn.SetActive(true);
        LeftBtn.SetActive(true);
    }
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
