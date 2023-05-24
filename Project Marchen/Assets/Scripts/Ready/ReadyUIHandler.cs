using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyUIHandler : NetworkBehaviour
{

    [Header("UI")]
    public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI countDownText;

    bool isReady = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void startGame()
    {

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);

            //Check if the player is ready
            if (!gameObjectToTransfer.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection)
                Runner.Disconnect(gameObjectToTransfer.GetComponent<NetworkObject>().InputAuthority);

        }
        Runner.SetActiveScene("TestScene(network)");
    }
    public void OnChangeWeaponHammer()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(0);
    }
    public void OnChangeWeaponGun()
    {
        if (isReady)
            return;
        NetworkPlayer.Local.GetComponent<AttackHandler>().ChangeWeapon(1);
    }

    public void OnReady()
    {
        if(Runner.IsServer)
        {
            
            startGame();
        }
    }

}
