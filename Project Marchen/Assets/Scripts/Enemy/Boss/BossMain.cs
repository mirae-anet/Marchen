using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMain : MonoBehaviour
{
    private BossController bossController;
    private MeshRenderer[] meshs;
    private SkinnedMeshRenderer skin;
    private Animator anim;

    private bool isDead = false;

    [Header("설정")]
    public bool Skinned = false;
    [Range(1, 1000)]
    public int maxHealth = 1000;
    [Range(1, 1000)]
    public int curHealth = 1000;
    [Range(0f, 5f)]
    public float knockbackForce = 0.3f;

    void Awake()
    {
        bossController = GetComponent<BossController>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        skin = GetComponentInChildren<SkinnedMeshRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PlayerBullet")  // 원거리 공격
        {
            gameObject.layer = 10;  // 슈퍼 아머

            BulletMain bulletMain = collision.gameObject.GetComponent<BulletMain>();
            curHealth -= bulletMain.damage;
            Vector3 reactDir = transform.position - collision.transform.position;
            reactDir.y = 0f;

            bossController.SetTarget(collision.gameObject.GetComponent<BulletMain>().GetParent()); // 발사한 객체로 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
            Destroy(collision.gameObject); // 피격된 불릿 파괴

            StartCoroutine(OnDamage(reactDir));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerAttack")  // 근접 공격
        {
            gameObject.layer = 10;  // 슈퍼 아머

            WeaponMain weaponMain = other.GetComponent<WeaponMain>();
            curHealth -= weaponMain.damage;
            Vector3 reactDir = transform.position - other.transform.position;
            reactDir.y = 0f;

            bossController.SetTarget(other.GetComponentInParent<Transform>().root); // 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
            //Debug.Log(other.GetComponentInParent<Transform>().root.ToString());
            StartCoroutine("OnDamage", reactDir);
        }
    }

    IEnumerator OnDamage(Vector3 reactDir)
    {
        Debug.Log(gameObject.name + " Hit!");
        bossController.setIsHit(true);
        anim.SetBool("isWalk", false);

        transform.position += reactDir * knockbackForce;

        if (!Skinned)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.red;
        }
        else
            skin.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        if (!Skinned)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white; // 몬스터의 원래 색깔로 변경
        }
        else
            skin.material.color = Color.white;

        if (curHealth <= 0)
            OnDie();
        else
            gameObject.layer = 8;  // 슈퍼 아머 해제

        bossController.setIsHit(false);
    }

    void OnDie()
    {
        gameObject.layer = 10;  // 슈퍼 아머
        isDead = true;

        //rigid.velocity = Vector3.zero;
        bossController.SetIsNavEnabled(false);

        anim.SetTrigger("doDie");

        //if (enemyType != Type.Boss)
        //    Destroy(gameObject, 3); // 3초 뒤에 삭제
    }

    public bool GetIsDead()
    {
        return isDead;
    }
}
