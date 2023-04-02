using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TPSCharacterController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigid;
    private Vector3 moveDir;

    [Header("������Ʈ ����")]

    [SerializeField]
    private Transform cameraArm;
    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private GameObject man;
    [SerializeField]
    private GameObject waffle;


    [Header("����")]

    [Range(1f, 5f)]
    public float cameraSpeed = 2f;
    //[SerializeField]
    [Range(5f, 10f)]
    public float characterSpeed = 5.5f;
    [Range(1f, 10f)]
    public float jumpPower = 10f;

    // ī�޶� ȸ�� �Լ�
    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X") * cameraSpeed, Input.GetAxis("Mouse Y") * cameraSpeed);   // ���콺 ������
        Vector3 camAngle   = cameraArm.rotation.eulerAngles;    // ī�޶� ��ġ ���� ���Ϸ� ������ ��ȯ

        float x = camAngle.x - mouseDelta.y;

        if (x < 180f)   // ���� 70�� ����
        {
            x = Mathf.Clamp(x, -1f, 70f);
        }
        else            // �Ʒ��� 25�� ����
        {
            x = Mathf.Clamp(x, 335f, 361f);
        }

        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);    // �� ȸ�� ��
    }

    private void Move()
    {
        if (!PlayerControl.isDead)
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));    // �̵� �Է� ������ moveInput ����
            bool isMove = moveInput.magnitude != 0; // moveInput�� ���̷� �Է� ����
            animator.SetBool("isMove", isMove);     // true�� �� �ȴ� �ִϸ��̼�, false�� �� ��� �ִϸ��̼�

            if (isMove && !PlayerControl.isJumping)
            {
                Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z);        // �ٶ󺸴� ���� ����
                Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized; // ������ ���ȭ
                moveDir = (lookForward * moveInput.y) + (lookRight * moveInput.x);                      // �ٶ󺸴� ���� ���� �̵� ����

                //characterBody.forward = lookForward;  // ĳ���� ����
                characterBody.forward = moveDir;        // ī�޶� ����        
            }

            transform.position += moveDir * Time.deltaTime * characterSpeed;    // ĳ���� �̵�
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))    // �����̽� Ŭ�� ��
        {
            if (!PlayerControl.isJumping) // ���� ���°� �ƴ� ��
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

        Invoke("Respawn", 3);   // 2�� �� ������ �Լ� ȣ��
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