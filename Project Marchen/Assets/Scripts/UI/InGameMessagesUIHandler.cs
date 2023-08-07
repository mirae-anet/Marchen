using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// @brief 게임 메시지를 표시하는 UI(채팅창).
public class InGameMessagesUIHandler : MonoBehaviour
{
    public TextMeshProUGUI[] textMeshProUGUIs;
    Queue messageQueue = new Queue();

    void Start()
    {
        
    }

    /// @brief 수신한 게임 메시지를 채팅창에 표시
    public void OnGameMessageReceived(string message) 
    {
        Debug.Log($"InGameMessagesUIHandler {message}");

        messageQueue.Enqueue(message);
        if(messageQueue.Count > 3)
            messageQueue.Dequeue();

        for (int i = textMeshProUGUIs.Length - 1; i > 0; i--)
        {
            textMeshProUGUIs[i].text = textMeshProUGUIs[i - 1].text;
        }
        textMeshProUGUIs[0].text = message;
    }   
}
