using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMain : MonoBehaviour
{
    [Header("설정")]
    public bool isMelee = false;
    [Range(1f, 30f)]
    public int damage = 10;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
            Destroy(gameObject, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall")
            Destroy(gameObject);
    }

    public int getDamage()
    {
        return damage;
    }
}