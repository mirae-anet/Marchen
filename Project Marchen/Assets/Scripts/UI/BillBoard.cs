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
        Camera[] cams = FindObjectsOfType<Camera>();
        if(cams.Length > 0)
        {
            for(int i = 0; i < cams.Length; i++)
            {
                if(cams[i] != null && cams[i].isActiveAndEnabled)
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
