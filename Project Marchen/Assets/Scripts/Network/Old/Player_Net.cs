using Fusion;
using UnityEngine;

namespace Fusion102
{
	public class Player_Net : NetworkBehaviour
	{
    	private Animator anim;
    	private Rigidbody rigid;

        [Header("오브젝트 연결")]
        [SerializeField]
        private Transform playerBody;

		[Header("설정")]
		[Range(1f, 30f)]
		public float moveSpeed = 15f;
		public bool onVelo = true;

		private void Awake()
		{
        	anim = GetComponentInChildren<Animator>();
        	rigid = GetComponent<Rigidbody>();
		}

		public override void FixedUpdateNetwork()
		{
			if (GetInput(out NetworkInputData data))
			{
				if(data.isMove)
				{
					data.moveDir.Normalize();

					//playerBody.forward = lookForward;  // 캐릭터 고정
					playerBody.forward = data.moveDir;        // 카메라 고정

					float walkSpeed = (data.walkOn ? 0.3f : 1f);
					Vector3 moveVec = data.moveDir * moveSpeed;
					if (onVelo)
					    rigid.velocity = new Vector3(moveVec.x, rigid.velocity.y, moveVec.z); // 물리 이동
					else
					    transform.position += moveVec * Time.deltaTime; // 절대 이동
				}

	            //animation 
	            anim.SetBool("isRun", data.isMove); // true일 때 걷는 애니메이션, false일 때 대기 애니메이션
	            anim.SetBool("isWalk", data.walkOn); // 최종적으로는 isMove, walkOn 둘 다 True 일 때 걷기
			}
		}
	}
}
