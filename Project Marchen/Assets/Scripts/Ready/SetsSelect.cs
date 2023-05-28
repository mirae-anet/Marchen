using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class SetsSelect : NetworkBehaviour
{
    private ReadyUIHandler readyUIHandler;
    public Camera localCamera;

    private void OnCollisionEnter(Collision collision)
    {
            if (collision.gameObject.CompareTag("Player"))
            {
                NetworkObject networkObject = collision.transform.root.GetComponent<NetworkObject>();
                
                if (Runner.IsServer && networkObject.HasInputAuthority)
                {
                    RPC_SetCanvas(true);
                    //RPC_NotCamera(true);
                }
            }
    }




    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCanvas(bool set)
    {
        readyUIHandler = FindObjectOfType<ReadyUIHandler>(true);
        if(readyUIHandler != null)
        {
            GameObject setCanvas = readyUIHandler.gameObject;
            setCanvas.SetActive(set);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotCamera(bool set)
    {
        localCamera = FindLocalCamera();
        LocalCameraHandler camerahandler = localCamera.GetComponentInParent<LocalCameraHandler>();
        camerahandler.localCameraEnable(set);
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
