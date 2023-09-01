using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 1.5f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }
}
