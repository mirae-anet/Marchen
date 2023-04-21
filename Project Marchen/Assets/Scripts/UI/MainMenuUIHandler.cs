using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUIHandler : MonoBehaviour
{
    public TMP_InputField inputField;

    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerNickname"))
            inputField.text = PlayerPrefs.GetString("PlayerNickname");
    }
    
    public void OnJoinGameClicked()
    {
        //PlayerPrefs는 게임 설정이나 데이터를 저장하고 로드하는 데 사용되는 기본적인 저장소입니다. 
        //이 클래스를 사용하면 게임이 종료되거나 기기가 종료된 후에도 데이터를 유지할 수 있습니다.
        PlayerPrefs.SetString("PlayerNickname", inputField.text);
        PlayerPrefs.Save(); //

        SceneManager.LoadScene("TestScene(network)");
    }
}
