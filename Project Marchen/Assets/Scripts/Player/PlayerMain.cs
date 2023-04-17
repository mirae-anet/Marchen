using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    public int health;
    public int maxHealth;

    bool isDamage;

    Rigidbody rigid;
    MeshRenderer[] meshs;
    GameObject nearObject;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
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
        
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                
                if (other != null && other.GetComponent<Rigidbody>() != null)
                {
                    Vector3 dirVec = (transform.position - other.transform.position).normalized;

                    rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
                    rigid.AddForce(dirVec.normalized * 50f, ForceMode.Impulse);

                    Destroy(other.gameObject);
                }

                StartCoroutine(OnDamage());
            }
        }
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
