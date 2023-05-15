using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera playerCamera;

    private Vector3 lookForward;
    private Vector3 lookRight;
    private Vector3 moveDir;

    private Vector3 aimForwardVec;
    private Vector3 hitPoint;
    private RaycastHit hit;

    //[Header("오브젝트 연결")]
    //[SerializeField]

    [Header("설정")]
    [Range(1f, 5f)]
    private float cameraSpeed = 2f;

    [Header("Layer for the aim raycast")]
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float maxDistance = 500;
    [SerializeField]
    private Transform bodyAnchorPoint;
    [SerializeField]
    private Transform bulletPos;


    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        aimForwardVec = bodyAnchorPoint.forward;
    }

    void Update()
    {
        CameraRotation();

        SetAimForwardVec();
        BulletLookAt();
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

    public Vector3 GetMoveDir(Vector2 moveInput)
    {
        lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // 정면 방향 저장
        lookRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;       // 좌우 방향 저장

        moveDir = (lookForward * moveInput.y) + (lookRight * moveInput.x); // 바라보는 방향 기준 이동 방향

        return moveDir;
    }

    //3차원에서 보는 바라보는 곳으로 aimForwardVec 갱신
    private void SetAimForwardVec()
    {
        if (Physics.Raycast(playerCamera.transform.position + playerCamera.transform.forward.normalized * 22, playerCamera.transform.forward, out hit, maxDistance, layerMask))
        {
            Debug.DrawRay(playerCamera.transform.position + playerCamera.transform.forward.normalized * 22, playerCamera.transform.forward * hit.distance, Color.yellow);
            hitPoint = hit.point;
            aimForwardVec = hitPoint - bodyAnchorPoint.position;
        }
        else
        {
            // hit nothing. 바라보고있는 방향의 maxDistance의 좌표
            Debug.DrawRay(playerCamera.transform.position + playerCamera.transform.forward.normalized * 22, playerCamera.transform.forward.normalized * maxDistance, Color.yellow);
            hitPoint = playerCamera.transform.position + playerCamera.transform.forward.normalized * (maxDistance + 22);
            aimForwardVec = hitPoint - bodyAnchorPoint.position;
        }
        aimForwardVec = aimForwardVec.normalized;
    }

    //public Vector3 GetAimForwardVec()
    //{
    //    return aimForwardVec;
    //}

    //public Vector3 GethitPoint()
    //{
    //    return hitPoint;
    //}

    private void BulletLookAt()
    {
        bulletPos.transform.LookAt(hitPoint);
    }
}
