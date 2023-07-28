using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 줍을 수 있는 아이템의 상호작용.
/// @see PlayerActionHandler
public class PickUpAction : InteractionHandler
{
    public enum Type { GreenBook, RedBook, BlueBattery, GreenBattary, Key};
    /// @brief 아이템의 타입 (배터리, 열쇠 등)
    [Header("설정")]
    public Type type;

    //other component
    /// @brief 해당 아이템을 스폰한 스폰너
    public NetworkObject Spawner;

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    /// @brief 접촉시 아이템을 습득. 일정시간 후 재생성하기 위해서 스폰너의 타이머를 재설정.
    private void OnTriggerEnter(Collider other)
    {
        if(Object != null && Object.HasStateAuthority)
        {
            if (other.tag == "Player")
            {
                PlayerActionHandler playerActionHandler = other.transform.root.GetComponent<PlayerActionHandler>();
                if(playerActionHandler != null)
                    playerActionHandler.action(transform);
    
                if(Spawner != null)
                {
                    Spawner.gameObject.SetActive(true);
                    Spawner.GetComponent<SpawnHandler>().SetTimer();
                }
                
                Runner.Despawn(Object);
            }
        }
    }
}
