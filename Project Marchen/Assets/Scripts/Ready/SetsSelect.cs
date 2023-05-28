using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class SetsSelect : NetworkBehaviour
{
    private ReadyUIHandler readyUIHandler;
    public Camera localCamera;
    CharacterInputHandler inputHandler;
    public GameObject ReadyUiCanvas;
    
    
    private void OnCollisionEnter(Collision collision)
    {
            if (collision.gameObject.CompareTag("Player"))
            {
                NetworkObject networkObject = collision.transform.root.GetComponent<NetworkObject>();
                //redyui핸들러가있을경우
                readyUIHandler = FindObjectOfType<ReadyUIHandler>();
                if (readyUIHandler != null) { return; }

                if (Runner.IsServer && networkObject.HasInputAuthority)
                {
                    
                    Runner.Spawn(ReadyUiCanvas);
                    RPC_SetCanvas(true);
                    RPC_NotCamera(false);
                }
            }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCanvas(bool set)
    {
        readyUIHandler = FindObjectOfType<ReadyUIHandler>(true);
        if(readyUIHandler != null)
        {
            inputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
            inputHandler.EnableinPut(false);

            GameObject setCanvas = readyUIHandler.gameObject;
            setCanvas.SetActive(set);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotCamera(bool set)
    {
        localCamera = FindLocalCamera();
        LocalCameraHandler camerahandler = localCamera.GetComponentInParent<LocalCameraHandler>();
        camerahandler.EnableRotationReady(set);
    }


    private Camera FindLocalCamera()
    {
        Camera[] cams = FindObjectsOfType<Camera>();
        if (cams.Length > 0)
        {
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams != null && cams[i].isActiveAndEnabled)
                {
                    return cams[i];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

}
