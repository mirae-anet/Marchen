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

                //if (other != null && other.GetComponent<Rigidbody>() != null)
                if (other != null)
                {
                    Vector3 dirVec = (transform.position - other.transform.position).normalized;

                    rigid.AddForce(Vector3.up * 25f, ForceMode.Impulse);
                    rigid.AddForce(dirVec * 10f, ForceMode.Impulse);

                    if (other.GetComponent<Rigidbody>() != null)
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

        yield return new WaitForSeconds(0.6f);

        isDamage = false;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
    }

    public bool getIsHit()
    {
        return isDamage;
    }
}
