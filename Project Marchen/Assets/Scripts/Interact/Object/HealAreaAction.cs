using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 낙사 체크.
public class HealAreaAction : MonoBehaviour
{
    public int amount;
    public float delay;
    Timer timer;

    private void Start() {
        timer = GetComponent<Timer>(); 
        timer.Reset(delay);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(timer.isTimeOut()){
            if(other.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
            {
                hpHandler.OnHeal(amount);
            }
            timer.Reset(delay);
        }
    }
}
