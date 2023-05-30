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
    EscHandler escHandler;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //호스트인경우
            NetworkObject networkObject = collision.transform.root.GetComponent<NetworkObject>();
            if (Runner.IsServer && networkObject.HasInputAuthority)
            {
                //캔버스가 없을경우
                readyUIHandler = FindObjectOfType<ReadyUIHandler>(true);
                if (readyUIHandler == null)
                {
                    Runner.Spawn(ReadyUiCanvas);
                    RPC_MouseSet(true);
                    RPC_RotateCamera(false);
                }
                else{ return; }
            }
        }
    }

    //마우스잠금
   [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
   public void RPC_MouseSet(bool set)
   {
        inputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
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
        localCamera = FindLocalCamera();
        LocalCameraHandler camerahandler = localCamera.GetComponentInParent<LocalCameraHandler>();
        camerahandler.EnableRotationReady(enable);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_LeftUi()
    {
        readyUIHandler = FindObjectOfType<ReadyUIHandler>(true);
        GameObject setCanvas = readyUIHandler.gameObject;
        NetworkObject canvasNetworkObject = setCanvas.GetComponent<NetworkObject>();
        Runner.Despawn(canvasNetworkObject);
    }

    public void LeftUI()
    {
        escHandler = FindObjectOfType<EscHandler>();
        if (escHandler.ActiveEsc())
        {
            RPC_LeftUi();
            RPC_RotateCamera(true);
            return;
        }
        RPC_LeftUi();
        RPC_MouseSet(false);
        RPC_RotateCamera(true);
    }

    private Camera FindLocalCamera()
    {
        Camera[] cams = FindObjectsOfType<Camera>();
        if (cams.Length > 0)
        {
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i] != null && cams[i].isActiveAndEnabled)
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
