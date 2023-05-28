using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetReponAreaAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        CharacterRespawnHandler characterRespawnHandler = other.transform.root.GetComponent<CharacterRespawnHandler>();
        if(characterRespawnHandler != null)
            characterRespawnHandler.ChangeSpawnPoint(transform.position);

        Destroy(transform.root.gameObject);
    }
}
