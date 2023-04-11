using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxManPlayerMain : MonoBehaviour
{
    public static bool isJumping = false;
    public static bool isDead = false;


    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AoE") || other.gameObject.CompareTag("OutLine"))
        {
            isDead = true;
        }
    }

}
