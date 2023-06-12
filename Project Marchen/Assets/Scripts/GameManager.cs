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
}
