using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMain : MonoBehaviour
{
    public enum Type { Melee, Range };

    [Header("오브젝트 연결")]
    [SerializeField]
    private BoxCollider meleeArea;
    [SerializeField]
    private TrailRenderer trailEffect;

    [Header("설정")]
    public Type type;
    [Range(1f, 100f)]
    public int damage = 25;
    [Range(0f, 5f)]
    public float delay = 0.4f;

    public void Attack()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    public float getDelay()
    {
        return delay;
    }
}
