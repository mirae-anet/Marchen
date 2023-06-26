using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @breif 로컬 카메라 관련 클래스
public class LocalCameraHandler : MonoBehaviour
{
    /// @breif 로컬 플레이어 각자의 카메라. static 변수.  
    static public LocalCameraHandler Local;

    [Header("설정")]
    [Range(1f, 500f)]
    public float cameraSpeed = 200f;

    [Header("Anchor Point")]
    public Transform cameraAnchorPoint;
    public Transform bodyAnchorPoint;
    public Transform BulletPos;

    [Header("Layer for the aim raycast")]
    /// @breif 조준할 수 있는 레이어
    public LayerMask layerMask;

    /// @breif 조준할 수 있는 최대 거리
    public float maxDistance;

    /// @breif 조준 방향
    Vector3 aimForwardVector;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //other component
    [SerializeField]
    private Camera localCamera;
    //ESC MENU
    private bool EscRotationEnabled = true;
    //READYUI
    private bool ReadyRotationEnabled = true;

    void Start()
    {
        transform.forward = GameManager.instance.cameraViewRotation;
        aimForwardVector = BulletPos.forward;
    }

    void LateUpdate()
    {
        CamControl();
        setAimForwardVector();
    }

    /// @breif 카메라 회전, 이동
    private void CamControl()
    {
        if(!localCamera.enabled)
        {
            return;
        }
        if (EscRotationEnabled && ReadyRotationEnabled)
        {
            cameraRotationX = viewInput.y *  cameraSpeed;
            cameraRotationY = viewInput.x *  cameraSpeed;
            Vector3 camAngle = transform.rotation.eulerAngles;

            float x = camAngle.x - cameraRotationX;

            if (x < 180f)
            {
                x = Mathf.Clamp(x, -1f, 70f); // 위쪽 70도 제한
            }
            else
            {
                x = Mathf.Clamp(x, 335f, 361f); // 아래쪽 25도 제한
            }

            transform.rotation = Quaternion.Euler(x, camAngle.y + cameraRotationY, camAngle.z); // 새 회전 값
        }

        // Move the cameraArm to the position of the player
        transform.position = cameraAnchorPoint.position;
    }

    /// @breif 조준점 계산. 3차원에서 보는 바라보는 곳으로 aimForwardVector 갱신
    private void setAimForwardVector()
    {
        if(Local != null)
            if(Local != this)
                return;

        Vector3 hitPoint;
        RaycastHit hit;

        if(Physics.Raycast(localCamera.transform.position + localCamera.transform.forward.normalized * 22 , localCamera.transform.forward, out hit, maxDistance, layerMask))
        {
            Debug.DrawRay(localCamera.transform.position + localCamera.transform.forward.normalized * 22, localCamera.transform.forward * hit.distance, Color.yellow);
            hitPoint = hit.point;
            aimForwardVector = hitPoint - BulletPos.position;
        }
        else
        {
            // hit nothing. 바라보고있는 방향의 maxDistance의 좌표
            Debug.DrawRay(localCamera.transform.position + localCamera.transform.forward.normalized * 22, localCamera.transform.forward.normalized * maxDistance, Color.yellow);
            hitPoint = localCamera.transform.position + localCamera.transform.forward.normalized * (maxDistance+22);
            aimForwardVector = hitPoint - BulletPos.position;
        }
        aimForwardVector = aimForwardVector.normalized;
    }

    /// @breif 마우스 이동에 관한 입력을 받아옴.
    /// @see CharacterInputHanlder
    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    /// @breif 조준점을 반환한다.
    /// @return Vector3 aimForwardVector
    /// @see CharacterInputHandler.GetNetworkInput()
    public Vector3 getAimForwardVector()
    {
        return aimForwardVector;
    }

    /// @breif 아바타의 이동에 관한 정보를 반환한다.
    /// @return Vector3 moveDir
    /// @see CharacterInputHandler.GetNetworkInput()
    public Vector3 getMoveDir(Vector2 moveInputVector)
    {
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // 정면 방향 저장
        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;       // 좌우 방향 저장

        Vector3 moveDir = (lookForward * moveInputVector.y) + (lookRight * moveInputVector.x); // 바라보는 방향 기준 이동 방향

        return moveDir;
    }

    /// @breif 연결된 카메라를 끄고 껸다.
    public void localCameraEnable(bool able)
    {
        localCamera.enabled = able;       
    }

    /// @breif ESC 메뉴의 활성화 여부에 따른 카메라 회전의 가능 여부를 갱신한다.
    public void EnableRotationEsc(bool enable)
    {
        EscRotationEnabled = enable;
    }

    /// @breif Ready 메뉴의 활성화 여부에 따른 카메라 회전의 가능 여부를 갱신한다.
    public void EnableRotationReady(bool enable)
    {
        ReadyRotationEnabled = enable;
    }

    /// @breif 카메라 파괴 시 바라보는 방향을 게임 매니저에 저장.
    /// @details 호스트마이그레이션 시에 저장된 정보를 바탕으로 바라보는 방향을 복원.
    private void OnDestroy()
    {
        //when localCamera was disabled
        GameManager.instance.cameraViewRotation = transform.forward;
    }
}
