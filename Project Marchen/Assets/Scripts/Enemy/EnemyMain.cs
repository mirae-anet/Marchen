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

    public enum Type { A, B };

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
    }

    void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "Melee")  // 근접 공격
        //{
        //    Weapon weapon = other.GetComponent<Weapon>();
        //    curHealth -= weapon.damage;
        //    Vector3 reactVec = transform.position - other.transform.position;

        //    StartCoroutine(OnDamage(reactVec));
        //}

        //else if (other.tag == "Bullet")  // 원거리 공격
        //{
        //    Bullet bullet = other.GetComponent<Bullet>();
        //    curHealth -= bullet.damage;
        //    Vector3 reactVec = transform.position - other.transform.position;

        //    Destroy(other.gameObject);

        //    StartCoroutine(OnDamage(reactVec));
        //}
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;  // 몬스터의 원래 색깔로 변경

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 2, ForceMode.Impulse);
        }

        else
        {
            mat.color = Color.gray;  // 몬스터가 죽으면 회색으로 변경
            gameObject.layer = 10;

            enemyController.setIsChase(false);
            enemyController.setNavEnabled(false);

            anim.SetTrigger("doDie");

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);

            Destroy(gameObject, 3);  // 3초 뒤에 삭제
        }
    }

    public Type getEnemyType()
    {
        return enemyType;
    }
}
