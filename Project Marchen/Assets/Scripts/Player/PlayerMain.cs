using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    public int health;
    public int maxHealth;

    bool isDamage;


    MeshRenderer[] meshs;
    GameObject nearObject;

    void Awake()
    {
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch (item.type)
            {
                case Item.Type.Heart:
                    health += item.value;

                    if (health > maxHealth)
                        health = maxHealth;

                    break;
            }

            Destroy(other.gameObject);
        }
        /* 나중에 플레이어 무기 코드 작성하면 이 코드 활성화
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                if (other.GetComponent<Rigidbody>() != null)
                    Destroy(other.gameObject);

                StartCoroutine(OnDamage());
            }
        }
        */
    }
    
    IEnumerator OnDamage()
    {
        isDamage = true;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.yellow;

        yield return new WaitForSeconds(1f);

        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }
}
