using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class PortalHandler : InteractionHandler
{
    public GameObject ReadyUiCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        NetworkObject networkObject = other.transform.root.GetComponent<NetworkObject>();
        if (Runner.IsServer && networkObject.HasInputAuthority)
        {
            ReadyUIHandler readyUIHandler = LocalCameraHandler.Local.GetComponentInChildren<ReadyUIHandler>(true);
            if (readyUIHandler != null && SceneManager.GetActiveScene().name == "Scene_2")
            {
                Debug.Log("On PortalHandler trigger");
                readyUIHandler.RPC_SetActiveReadyUI(true);
                readyUIHandler.RPC_MouseSet(true);
                readyUIHandler.RPC_RotateCamera(false);
                readyUIHandler.SetActive();

            }
            else
            {
                readyUIHandler.startGame();
            }
        }
    }
}
