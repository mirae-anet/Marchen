using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timeRemaining = 0;
    private bool timeOut = false;
    public bool timerIsRunning = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                timeRemaining = 0;
                TimeOut();
            }
        }
    }
    void TimeOut(){
        Debug.Log("Time out!");
        timerIsRunning = false;
        timeOut = true;
    }

    public void Reset(float time)
    {
        timeOut=false;
        timerIsRunning = true;
        this.timeRemaining = time;
    }
    public bool isTimeOut(){
        return timeOut;
    }
}
