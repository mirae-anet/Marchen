using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TPSCharacterController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigid;
    private Vector3 moveDir;

    [Header("오브젝트 연결")]

    [SerializeField]
    private Transform cameraArm;
    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private GameObject man;
    [SerializeField]
    private GameObject waffle;


    [Header("설정")]

    [Range(1f, 5f)]
    public float cameraSpeed = 2f;
    //[SerializeField]
    [Range(5f, 10f)]
    public float characterSpeed = 5.5f;
    [Range(1f, 10f)]
    public float jumpPower = 10f;

    // 카메라 회전 함수
    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X") * cameraSpeed, Input.GetAxis("Mouse Y") * cameraSpeed);   // 마우스 움직임
        Vector3 camAngle   = cameraArm.rotation.eulerAngles;    // 카메라 위치 값을 오일러 각으로 변환

        float x = camAngle.x - mouseDelta.y;

        if (x < 180f)   // 위쪽 70도 제한
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }
        else            // 아래쪽 25도 제한
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);    // 새 회전 값
    }

    private void Move()
    {
        if (!PlayerControl.isDead)
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));    // 이동 입력 값으로 moveInput 벡터
            bool isMove = moveInput.magnitude != 0; // moveInput의 길이로 입력 판정
            animator.SetBool("isMove", isMove);     // true일 때 걷는 애니메이션, false일 때 대기 애니메이션

            if (isMove && !PlayerControl.isJumping)
            {
                Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z);        // 바라보는 방향 저장
                Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized; // 오른쪽 평면화
                moveDir = (lookForward * moveInput.y) + (lookRight * moveInput.x);                      // 바라보는 방향 기준 이동 방향

                //characterBody.forward = lookForward;  // 캐릭터 고정
                characterBody.forward = moveDir;        // 카메라 고정        
            }

            transform.position += moveDir * Time.deltaTime * characterSpeed;    // 캐릭터 이동
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))    // 스페이스 클릭 시
        {
            if (!PlayerControl.isJumping) // 점프 상태가 아닐 시
            {
                PlayerControl.isJumping = true;
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
            else
            {
                return;
            }
        }
    }

    private void Dead()
    {
        man.SetActive(false);
        waffle.SetActive(true);
        PlayerControl.isDead = true;

        Invoke("Respawn", 3);   // 2초 뒤 리스폰 함수 호출
    }

    private void Respawn()
    {
        PlayerControl.isDead = false;
        SceneManager.LoadScene("Stage");
    }

    void Start()
    {
        animator = characterBody.GetComponent<Animator>();
        rigid = characterBody.GetComponent<Rigidbody>();
    }

    void Update()
    {
        LookAround();
        Move();
        Jump();

        if (PlayerControl.isDead == true)
        {
            Dead();
        }
    }

}