using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Timeline.AnimationPlayableAsset;

 namespace litttle_Dog_scripits
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController characterController;
        public float PlayerSpeed = 12f;
        public static bool canMove = true;
        // Start is called before the first frame update
        void Start()
        {

            characterController = GetComponent<CharacterController>();
            MouseControl();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                canMove = !canMove;
                MouseControl();
            }
            if (canMove == true)
            {
                float X = Input.GetAxis("Horizontal");
                float Z = Input.GetAxis("Vertical");
                Vector3 Move = transform.right * X + transform.forward * Z;
                characterController.Move(Move * PlayerSpeed * Time.deltaTime);

            }

        }
        public void MouseControl()
        {
            if (canMove == false)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
    }
}
