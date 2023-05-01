using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    [Header("Anchor Point")]
    // [SerializeField]
    // Transform cameraAnchorPoint;
    [SerializeField]
    Transform bodyAnchorPoint;

    [Header("Layer for the aim raycast")]
    [SerializeField]
    LayerMask layerMask;
    [SerializeField]
    float maxDistance;
    Vector3 aimForwardVector;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //other component
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    CharacterMovementHandler characterMovementHandler;
    private Camera localCamera;

    void Awake()
    {
      localCamera = GetComponentInChildren<Camera>();  
      networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
      characterMovementHandler = GetComponentInParent<CharacterMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraRotationX = GameManager.instance.cameraViewRotation.x;
        cameraRotationY = GameManager.instance.cameraViewRotation.y;
        aimForwardVector = bodyAnchorPoint.forward;
    }

    private void Update()
    {
        //조종하는 사람만 실행
        if(!characterMovementHandler.Object.HasInputAuthority){return;} 
        
        setAimForwardVector();
    }

    void LateUpdate()
    {
        if(!localCamera.enabled)
        {
            return;
        }
        //new
        cameraRotationX = viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationY = viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;
        Vector3 camAngle = transform.rotation.eulerAngles;    // 카메라 위치 값을 오일러 각으로 변환

        float x = camAngle.x - cameraRotationX;

        if (x < 180f)   // 위쪽 70도 제한
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }
        else            // 아래쪽 25도 제한
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        transform.rotation = Quaternion.Euler(x, camAngle.y + cameraRotationY, camAngle.z);    // 새 회전 값

        /*
        //old
        //Move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;
        //Calculate rotation
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;
        //Apply rotation
        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        */
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    private void OnDestroy()
    {
        //when localCamera was disabled
        if(cameraRotationX != 0 && cameraRotationY != 0)         
        {
            GameManager.instance.cameraViewRotation.x = cameraRotationX;
            GameManager.instance.cameraViewRotation.y = cameraRotationY;
        }
    }
    public Vector3 getMoveDir(Vector2 moveInputVector)
    {
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // 정면 방향 저장
        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;       // 좌우 방향 저장

        Vector3 moveDir = (lookForward * moveInputVector.y) + (lookRight * moveInputVector.x); // 바라보는 방향 기준 이동 방향

        return moveDir;
    }

    //3차원에서 보는 바라보는 곳으로 aimForwardVector 갱신
    private void setAimForwardVector()
    {
        Vector3 hitPoint;
        RaycastHit hit;

        if(Physics.Raycast(localCamera.transform.position + localCamera.transform.forward.normalized * 22 , localCamera.transform.forward, out hit, maxDistance, layerMask))
        {
            Debug.DrawRay(localCamera.transform.position + localCamera.transform.forward.normalized * 22, localCamera.transform.forward * hit.distance, Color.yellow);
            hitPoint = hit.point;
            aimForwardVector = hitPoint - bodyAnchorPoint.position;
        }
        else
        {
            // hit nothing. 바라보고있는 방향의 maxDistance의 좌표
            Debug.DrawRay(localCamera.transform.position + localCamera.transform.forward.normalized * 22, localCamera.transform.forward.normalized * maxDistance, Color.yellow);
            hitPoint = localCamera.transform.position + localCamera.transform.forward.normalized * (maxDistance+22);
            aimForwardVector = hitPoint - bodyAnchorPoint.position;
        }
        aimForwardVector = aimForwardVector.normalized;
    }
    public Vector3 getAimForwardVector()
    {
        return aimForwardVector;
    }

    public void localCameraEnable(bool able)
    {
        localCamera.enabled = able;       
    }
}
