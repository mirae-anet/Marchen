using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace litttle_Dog_scripits
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseSensativty = 100f;
        float XRot = 0f;
        public Transform playerBody;
        /// 0 min 1 max 
        public float[] RotClamp = new float[2] { -90f, 90f };
        // Start is called before the first frame update
        void Start()
        {
            playerBody = transform.parent;
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerController.canMove == true)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensativty * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensativty * Time.deltaTime;
                XRot -= mouseY;
                XRot = Mathf.Clamp(XRot, RotClamp[0], RotClamp[1]);
                transform.localRotation = Quaternion.Euler(XRot, 0f, 0f);
                playerBody.Rotate(Vector3.up * mouseX);
            }


        }
    }

}


