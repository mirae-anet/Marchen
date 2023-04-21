using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameMessagesHandler : MonoBehaviour
{
    public TextMeshProUGUI[] textMeshProUGUIs;
    Queue messageQueue = new Queue();

    void Start()
    {
        
    }

    public void OnGameMessageReceived(string message) 
    {
        Debug.Log($"InGameMessagesUIHandler {message}");

        messageQueue.Enqueue(message);
        if(messageQueue.Count > 3)
            messageQueue.Dequeue();
        
        int queueIndex = 0;
        foreach(string messageInQueue in messageQueue)
        {
            textMeshProUGUIs[queueIndex].text = messageInQueue;
            queueIndex++;
        }
    }   
}
