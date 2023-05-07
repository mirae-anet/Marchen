using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMain : MonoBehaviour
{
    private Transform parentObject;

    [Header("설정")]
    [Range(1f, 30f)]
    public int damage = 10;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Wall")
            Destroy(gameObject, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
            
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetParent(Transform transform)
    {
        parentObject = transform;
        //Debug.Log(parentObject.name);
    }

    public Transform GetParent()
    { 
        return parentObject;
    }
}