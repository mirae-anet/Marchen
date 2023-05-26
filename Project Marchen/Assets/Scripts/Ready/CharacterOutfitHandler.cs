using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterOutfitHandler : NetworkBehaviour
{
    public NetworkBool isDoneWithCharacterSelection { get; set; }

    [Networked(OnChanged =nameof(OnIsDoneWithCharacterSelectionChanged))]
    public NetworkBool isDoneWidthCharacterSelection { get; set; }

    [Header("Ready UI")]
    public Image readyCheckboxImage;
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
            return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void IsDoneWithCharacterSelectionChanged()
    {
        if (isDoneWidthCharacterSelection)
            readyCheckboxImage.gameObject.SetActive(true);
        else readyCheckboxImage.gameObject.SetActive(false);
    }

    public void OnReady(bool isReady)
    {
        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
        {
            RPC_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithCharacterSelection = isReady;
    }


    static void OnIsDoneWithCharacterSelectionChanged(Changed<CharacterOutfitHandler> changed)
    {
        changed.Behaviour.IsDoneWithCharacterSelectionChanged();
    }


}
