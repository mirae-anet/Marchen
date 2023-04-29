using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    [Header("Anchor Point")]
    [SerializeField]
    Transform cameraAnchorPoint;
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
    public Camera localCamera;

    void Awake()
    {
      localCamera = GetComponent<Camera>();  
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
        if(cameraAnchorPoint == null)
        {
            return;
        }
        if(!localCamera.enabled)
        {
            return;
        }
        //Move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;
        //Calculate rotation
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;
        //Apply rotation
        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
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

    //3차원에서 보는 바라보는 곳으로 aimForwardVector 갱신
    private void setAimForwardVector()
    {
        Vector3 hitPoint;
        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, layerMask))
        {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);
            hitPoint = hit.point;
            aimForwardVector = hitPoint - bodyAnchorPoint.position;
        }
        else
        {
            // hit nothing. 바라보고있는 방향의 maxDistance의 좌표
            Debug.DrawRay(transform.position, transform.forward.normalized * maxDistance, Color.yellow);
            hitPoint = transform.position + transform.forward.normalized * maxDistance;
            aimForwardVector = hitPoint - bodyAnchorPoint.position;
        }
        aimForwardVector = aimForwardVector.normalized;
    }
    public Vector3 getAimForwardVector()
    {
        return aimForwardVector;
    }
}
