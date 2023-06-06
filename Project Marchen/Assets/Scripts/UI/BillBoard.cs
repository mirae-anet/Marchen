using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

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
