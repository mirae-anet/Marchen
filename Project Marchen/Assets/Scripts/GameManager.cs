using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Static instance of GameManager so other scripts can access it
    public static GameManager instance = null;

    byte[] connectionToken;
    public Vector3 cameraViewRotation = Vector3.zero; //For host migration 카메라가 보던 곳 그대로
    public string playerNickName = "";

    private DataManager dataManager;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        dataManager = GetComponent<DataManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(connectionToken == null)
            connectionToken = ConnectionTokenUtils.NewToken();
            Debug.Log($"Player connection token  {ConnectionTokenUtils.HashToken(connectionToken)}");
    }

    public void SetConnectionToken(byte[] connectionToken)
    {
        this.connectionToken = connectionToken;
    }

    public byte[] GetConnectionToken()
    {
        return connectionToken;
    }

    /* -------------- 세이브 관련 -------------- */
    [Header("스테이지 클리어 여부")]
    [SerializeField]
    private int clearStage = 0; // 0:초기, 1:Alice클리어, 2:Desert클리어

    public int ClearStage
    {
        get { return clearStage; }
        set
        {
            clearStage = value;
            dataManager.Save();
        }
    }

    public void AliceStageClear()
    {
        if (ClearStage < 1)
            ClearStage = 1;
    }

    public void DesertStageClear()
    {
        if (ClearStage < 2)
            ClearStage = 2;
    }
}
