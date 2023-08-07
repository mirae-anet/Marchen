using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 게임 UI가 플레이어를 향하도록 회전함.
public class BillBoard : MonoBehaviour
{
    public Camera localCamera;
    private void Start() 
    {
        localCamera = FindLocalCamera();
    }
    void LateUpdate()
    {
        if(localCamera != null && localCamera.isActiveAndEnabled)
        {
            transform.LookAt(transform.position + localCamera.transform.forward);
        }
        else
        {
            Debug.Log("BillBoard camera null");
            localCamera = FindLocalCamera();
        }
    }
    /// @brief 로컬 플레이어의 카메라를 찾음.
    private Camera FindLocalCamera()
    {
        if(LocalCameraHandler.Local != null)
        {
            Camera localCamera = LocalCameraHandler.Local.GetComponentInChildren<Camera>();
            return localCamera;
        }
        else
        {
            return null;
        }
    }
}
