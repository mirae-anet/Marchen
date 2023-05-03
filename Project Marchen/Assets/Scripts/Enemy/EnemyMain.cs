using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMain : MonoBehaviour
{
    private EnemyController enemyController;
    private Rigidbody rigid;
    private BoxCollider boxCollider;
    private Material mat;
    private Animator anim;

    public enum Type { Melee, Range };

    [Header("설정")]
    public Type enemyType;
    [Range(1f, 1000f)]
    public int maxHealth = 100;
    [Range(1f, 1000f)]
    public int curHealth = 100;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerAttack")  // 근접 공격
        {
            WeaponMain weaponMain = other.GetComponent<WeaponMain>();
            curHealth -= weaponMain.damage;
            Vector3 reactDir = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactDir));
        }

        else if (other.tag == "PlayerBullet")  // 원거리 공격
        {
            BulletMain bulletMain = other.GetComponent<BulletMain>();
            curHealth -= bulletMain.damage;
            Vector3 reactDir = transform.position - other.transform.position;

            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactDir));
        }
    }

    IEnumerator OnDamage(Vector3 reactDir)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;  // 몬스터의 원래 색깔로 변경

            reactDir = reactDir.normalized;
            reactDir += Vector3.up;
            rigid.AddForce(reactDir * 2, ForceMode.Impulse);
        }

        else
        {
            mat.color = Color.gray;  // 몬스터가 죽으면 회색으로 변경
            gameObject.layer = 10;

            enemyController.setIsChase(false);
            enemyController.setNavEnabled(false);

            anim.SetTrigger("doDie");

            reactDir = reactDir.normalized;
            reactDir += Vector3.up;
            rigid.AddForce(reactDir * 5, ForceMode.Impulse);

            Destroy(gameObject, 3);  // 3초 뒤에 삭제
        }
    }

    public Type getEnemyType()
    {
        return enemyType;
    }
}
