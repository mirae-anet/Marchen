using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : BulletMain
{
    float angularPower = 2;
    float scaleValue = 0.1f;

    bool isShoot;

    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f);

        isShoot = true;
    }

    IEnumerator GainPower()
    {
        while(!isShoot)
        {
            angularPower += 0.02f;
            scaleValue += 0.005f;

            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);

            yield return new WaitForSeconds(0.01f);
        }
    }
}