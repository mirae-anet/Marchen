using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterWeponHandler : MonoBehaviour
{

    struct NetworkOutfit : INetworkStruct
    {
        public byte weponId;
    }
    NetworkOutfit networkOutfit { get; set; }
    void Start()
    {
        
    }



    
}
