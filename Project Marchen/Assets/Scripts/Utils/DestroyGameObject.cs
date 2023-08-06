using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 생성된 후 일정시간 후에 파괴.
public class DestroyGameObject : MonoBehaviour
{
    /// @brief 수명
    [SerializeField]
    private float lifeTime = 1.5f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }
}
