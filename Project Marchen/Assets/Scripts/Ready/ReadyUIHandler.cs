using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyUIHandler : NetworkBehaviour
{

    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;
    public GameObject ReadyUiCanvas;
    public GameObject StartBtn;
    public GameObject LeftBtn;

    bool isReady = false;
    SetsSelect setslect;
    LocalCameraHandler local;
    //countdown
    TickTimer countdownTickTimer = TickTimer.None;

    [Networked(OnChanged = nameof(OnCountdownChanged))]
    byte countDown { get; set; }


    void Start()
    {
        countDownText.text = "";
        setslect = FindObjectOfType<SetsSelect>();
        local = FindObjectOfType<LocalCameraHandler>(); // 임시
    }

    // Update is called once per frame
    void Update()
    {
        if (countdownTickTimer.Expired(Runner))
        {
            startGame();

            countdownTickTimer = TickTimer.None;

        }
        else if(countdownTickTimer.IsRunning)
        {
            countDown = (byte)countdownTickTimer.RemainingTime(Runner);
        }
    }


    void startGame()
    {
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);
        }

        Runner.SetActiveScene("TestScene(network)orginal");
    }
    public void OnChangeWeaponHammer()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(0);
    }
    public void OnChangeWeaponGun()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(1);
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
                countdownTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
                buttonReadyText.text = "취소";
            }
            else
            {
                countdownTickTimer = TickTimer.None;
                countDown = 0;
                buttonReadyText.text = "게임시작";
            }
        }
        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }

    static void OnCountdownChanged(Changed<ReadyUIHandler> changed)
    {
        changed.Behaviour.OnCountdownChanged();
    }

    private void OnCountdownChanged()
    {
        if (countDown == 0)
            countDownText.text = $"";
        else countDownText.text = $"Game start in {countDown}";
    }

    public void LeftUi()
    {
        setslect.LeftUI();
    }

    public void SetActive()
    {
        StartBtn.SetActive(true);
        LeftBtn.SetActive(true);
    }
}
