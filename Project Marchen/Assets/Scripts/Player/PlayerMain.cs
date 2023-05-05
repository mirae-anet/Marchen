using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rigid;
    private MeshRenderer[] meshs;
    private GameObject nearObject;
    private WeaponMain weaponMain;

    private bool isDamage = false;
    private bool isDead = false;

    public enum Type { Hammer, Gun };

    // 인스펙터
    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject playerBody;
    [SerializeField]
    private GameObject[] weapons;

    [Header("설정")]
    public Type weaponType;
    [Range(1f, 100f)]
    public int health = 100;
    [Range(1f, 100f)]
    public int maxHealth = 100;
    
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        WeaponEquip();
    }

    void WeaponEquip()
    {
        switch (weaponType)
        {
            case Type.Hammer:
                weapons[0].SetActive(true);
                weapons[1].SetActive(false);
                weaponMain = weapons[0].GetComponent<WeaponMain>();
                break;

            case Type.Gun:
                weapons[0].SetActive(false);
                weapons[1].SetActive(true);
                weaponMain = weapons[1].GetComponent<WeaponMain>();
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch (item.getType())
            {
                case Item.Type.Heart:
                    health += item.getValue();
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
            }

            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyAttack" || other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                BulletMain enemyBullet = other.GetComponent<BulletMain>();
                health -= enemyBullet.getDamage();
                //Debug.Log(other.GetComponent<BulletMain>().getParent());

                //if (other != null && other.GetComponent<Rigidbody>() != null)
                if (other != null)
                {
                    Vector3 reactDir = (transform.position - other.transform.position).normalized;

                    rigid.AddForce(Vector3.up * 25f, ForceMode.Impulse);
                    rigid.AddForce(reactDir * 10f, ForceMode.Impulse);

                    if (other.GetComponent<Rigidbody>() != null)
                        Destroy(other.gameObject);
                }

                StartCoroutine(OnDamage());
            }
        }
    }
    
    IEnumerator OnDamage()
    {
        if (health <= 0)
            OnDie();

        isDamage = true;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.yellow;

        yield return new WaitForSeconds(0.6f);

        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }

    void OnDie()
    {
        gameObject.tag = "Respawn"; // Player 태그 갖고 있으면 Enemy 타겟팅 망가짐
        playerBody.layer = 10; // 슈퍼아머
        rigid.velocity = Vector3.zero;
        anim.SetTrigger("doDie");

        isDead = true;
    }

    public bool getIsHit()
    {
        return isDamage;
    }

    public bool getIsDead()
    {
        return isDead;
    }

    public WeaponMain GetWeaponMain()
    {
        return weaponMain;
    }
}
