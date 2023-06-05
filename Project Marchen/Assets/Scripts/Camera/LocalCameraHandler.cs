using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    static public LocalCameraHandler Local;

    [Header("설정")]
    [Range(100f, 500f)]
    public float cameraSpeed = 200f;

    [Header("Anchor Point")]
    public Transform cameraAnchorPoint;
    public Transform bodyAnchorPoint;
    public Transform BulletPos;

    [Header("Layer for the aim raycast")]
    public LayerMask layerMask;
    public float maxDistance;
    Vector3 aimForwardVector;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //other component
    CharacterRespawnHandler characterRespawnHandler;

    [SerializeField]
    private Camera localCamera;
    //ESC MENU
    private bool EscRotationEnabled = true;
    //READYUI
    private bool ReadyRotationEnabled = true;

    // Start is called before the first frame update
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

    private void CamControl()
    {
        if(!localCamera.enabled)
        {
            return;
        }
        if (EscRotationEnabled && ReadyRotationEnabled)
        {
            cameraRotationX = viewInput.y * Time.deltaTime * cameraSpeed;
            cameraRotationY = viewInput.x * Time.deltaTime * cameraSpeed;
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

    //3차원에서 보는 바라보는 곳으로 aimForwardVector 갱신
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

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    public Vector3 getAimForwardVector()
    {
        return aimForwardVector;
    }

    public Vector3 getMoveDir(Vector2 moveInputVector)
    {
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // 정면 방향 저장
        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;       // 좌우 방향 저장

        Vector3 moveDir = (lookForward * moveInputVector.y) + (lookRight * moveInputVector.x); // 바라보는 방향 기준 이동 방향

        return moveDir;
    }

    public void localCameraEnable(bool able)
    {
        localCamera.enabled = able;       
    }

    public void EnableRotationEsc(bool enable)
    {
        EscRotationEnabled = enable;
    }

    public void EnableRotationReady(bool enable)
    {
        ReadyRotationEnabled = enable;
    }

    private void OnDestroy()
    {
        //when localCamera was disabled
        GameManager.instance.cameraViewRotation = transform.forward;
    }
}
