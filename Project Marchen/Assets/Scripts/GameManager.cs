using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 게임매니저
public class GameManager : MonoBehaviour
{
    /// @brief Static instance of GameManager so other scripts can access it
    public static GameManager instance = null;

    /// @brief connection Token을 생성하고 저장하는 변수
    byte[] connectionToken;
    /// @brief For host migration 카메라가 보던 곳 그대로 이어서 바라보게 하기 위해서 저장
    public Vector3 cameraViewRotation = Vector3.zero; 
    /// @brief 다음에 접속하면 이전에 사용한 닉네임이 저장되어있음.
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

    /// @brief connection token을 생성한다.
    void Start()
    {
        if(connectionToken == null)
            connectionToken = ConnectionTokenUtils.NewToken();
            Debug.Log($"Player connection token  {ConnectionTokenUtils.HashToken(connectionToken)}");
    }

    /// @brief connection token을 setting한다.
    public void SetConnectionToken(byte[] connectionToken)
    {
        this.connectionToken = connectionToken;
    }

    /// @brief 생성한 Connection Token에 접근할 수 있음.
    /// @see NetworkRunnerHandler
    public byte[] GetConnectionToken()
    {
        return connectionToken;
    }
}
