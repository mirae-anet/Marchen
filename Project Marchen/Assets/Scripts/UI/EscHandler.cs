using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//@brief ESC메뉴를 다루는 스크립트
public class EscHandler : NetworkBehaviour
{
    //@brief ESC패널, 카매라 회전, 입력제어를 위한 변수
    public GameObject escPanel;
    LocalCameraHandler localCameraHandler;
    CharacterInputHandler inputHandler;
    private ReadyUIHandler readyUIHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInParent<LocalCameraHandler>();
        inputHandler = GetComponentInParent<CharacterInputHandler>();
    }

    private void Update()
    {
        EscMenu();
    }

    //@brief 방 나가기
    public void ExitRoom()
    {
        Runner.Shutdown();
        SceneManager.LoadScene("Scene_1");
    }

    //@brief 게임 종료
    public void ExitGame()
    {
        Application.Quit();
    }

    //@brief ESC 화면 설정
    public void EscMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //카메라
            readyUIHandler = FindObjectOfType<ReadyUIHandler>();

            if (escPanel.activeSelf) //ESC 패널이 켜져있으면
            {

                if (readyUIHandler != null)  //READYUI가 켜져있을경우
                {
                    escPanel.SetActive(false);
                    localCameraHandler.EnableRotationEsc(true);
                    return;
                }
                else // 일반적일 경우
                {
                    escPanel.SetActive(false);
                    localCameraHandler.EnableRotationEsc(true);
                    inputHandler.EnableinPut(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else //ESC 패널이 꺼져있을 경우
            {
                if (readyUIHandler != null) //READYUI가 켜져있을경우
                {
                    escPanel.SetActive(true);
                    localCameraHandler.EnableRotationEsc(false);
                    return;
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                escPanel.SetActive(true);
                localCameraHandler.EnableRotationEsc(false);
                inputHandler.EnableinPut(false);
            }
        }
    }

    //ESC 패널 활성화 여부 확인
    public bool ActiveEsc()
    {
        return escPanel.activeSelf;
    }

}
