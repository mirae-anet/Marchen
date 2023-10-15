using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 낙사 체크.
public class HealAreaAction : MonoBehaviour
{
    public int amount;
    public float delay;
    Timer timer;
    private List<HPHandler> ObjectIn = new List<HPHandler>();

    private void Start() {
        timer = GetComponent<Timer>(); 
        timer.Reset(delay);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag != "Player")
            return;
        if(other.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
            ObjectIn.Add(hpHandler);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(timer.isTimeOut()){
            foreach (HPHandler hpHandler in ObjectIn)
            {
                if(hpHandler != null)
                    hpHandler.OnHeal(amount);
            }
            timer.Reset(delay);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag != "Player")
            return;
        if(other.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
            ObjectIn.Remove(hpHandler);
    }

}
