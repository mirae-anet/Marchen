using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //[Header("오브젝트 연결")]
    //[SerializeField]
    //private GameObject player;

    [Header("설정")]
    [Range(1f, 5f)]
    public float cameraSpeed = 2f;

    void Awake()
    {
        //player = GetComponent<Transform>();
    }

    void Update()
    {
        CameraRotation();
        //CameraMove();
    }

    private void CameraRotation()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X") * cameraSpeed, Input.GetAxis("Mouse Y") * cameraSpeed);   // 마우스 움직임
        Vector3 camAngle = transform.rotation.eulerAngles;    // 카메라 위치 값을 오일러 각으로 변환

        float x = camAngle.x - mouseDelta.y;

        if (x < 180f)   // 위쪽 70도 제한
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }
        else            // 아래쪽 25도 제한
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        transform.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);    // 새 회전 값
    }

    //private void CameraMove()
    //{
    //    transform.position = player.transform.position;
    //}

    public Vector3 GetMoveDir(Vector2 moveInput)
    {
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // 정면 방향 저장
        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;       // 좌우 방향 저장

        Vector3 moveDir = (lookForward * moveInput.y) + (lookRight * moveInput.x); // 바라보는 방향 기준 이동 방향

        return moveDir;
    }
}
