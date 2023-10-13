using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

/// @brief 포탈 트리거
public class PortalHandler : InteractionHandler
{
    public GameObject ReadyUiCanvas;

    private void OnTriggerEnter(Collider other)
    {
        //플레이어가 아닐경우 종료
        if (other.tag != "Player")
            return;
        NetworkObject networkObject = other.transform.root.GetComponent<NetworkObject>();
        if (Runner.IsServer && networkObject.HasInputAuthority) //플레이어가 호스트일 경우
        {
            ReadyUIHandler readyUIHandler = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>(true);

            //readyUi태그 변경
            if (gameObject.CompareTag("Alice"))
            {
                readyUIHandler.gameObject.tag = "Alice";

            }
            else
            {
                readyUIHandler.gameObject.tag = "Desert";
            }
            
            //플레이어 위치가 도서관일 경우
            if (readyUIHandler != null && SceneManager.GetActiveScene().name == "Scene_2" )
            {
                // 1스테이지 클리어시 사막 맵 입장 가능
                if(GameManager.instance.ClearStage<1 && gameObject.CompareTag("Desert"))
                {
                    return;
                }
                Debug.Log("On PortalHandler trigger");
                readyUIHandler.RPC_SetActiveReadyUI(true);
                readyUIHandler.RPC_MouseSet(true);
                readyUIHandler.RPC_RotateCamera(false);
                readyUIHandler.SetActive();

            }//플레이어 위치가 도서관이 아닐경우 복귀
            else
            {
                readyUIHandler.startGame();
            }
        }
    }
}
