using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMain : MonoBehaviour
{
    private Transform parentObject;

    [Header("설정")]
    [Range(0f, 30f)]
    public int damage = 10;
    [SerializeField]
    private bool groundDestroy = false;
    [SerializeField]
    private bool isMelee = false;

    private void Start()
    {
        if (isMelee)
            return;

        StartCoroutine(BulletDestroy(6f));
    }

    IEnumerator BulletDestroy(float second)
    {
        yield return new WaitForSeconds(second);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isMelee || !groundDestroy)
            return;

        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Wall")
            Destroy(gameObject, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isMelee)
            return;

        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Player")
            Destroy(gameObject);
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