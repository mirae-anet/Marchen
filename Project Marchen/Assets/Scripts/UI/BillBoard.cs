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
        return LocalCameraHandler.Local.GetComponentInChildren<Camera>();
    }
}
