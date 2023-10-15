using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 플레이어의 리스폰 위치를 갱신.
/// @details 실행시 모든 팀원의 리스폰 위치를 갱신함.
/// @see CharacterRespawnHandler.ChangeSpawnPoint()
public class SetSpawnAreaAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        CharacterRespawnHandler characterRespawnHandler = other.transform.root.GetComponent<CharacterRespawnHandler>();
        if(characterRespawnHandler != null)
            characterRespawnHandler.ChangeSpawnPoint(transform.position);

        Destroy(this.gameObject);
    }
}
