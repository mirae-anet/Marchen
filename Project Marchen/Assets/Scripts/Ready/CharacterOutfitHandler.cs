using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOutfitHandler : MonoBehaviour
{
    public NetworkBool isDoneWithCharacterSelection { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithCharacterSelection = isReady;
    }
}
