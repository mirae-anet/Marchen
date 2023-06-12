using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground" || other.tag == "Wall")
        {
            gameObject.GetComponentInParent<PlayerController>().SetIsGrounded(true);
        }
    }
}
