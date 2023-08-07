using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 각각의 클라이언트에서 서버에게 전달할 데이터구조.
/// @see NetworkPlayerController, CharacterInputHandler
public struct NetworkInputData : INetworkInput
{
    // public Vector3 lookForwardVector; //old
    // public Vector2 movementInput;

    /// @brief 이동값
    public Vector3 moveDir;
    /// @brief 이동 여부
    public NetworkBool isMove;
    /// @brief 점프 입력
    public NetworkBool jumpInput;
    /// @brief  걷기 여부
    public NetworkBool walkInput;
    /// @brief 구르기 여부
    public NetworkBool dodgeInput;
    /// @brief 공격 여부
    public NetworkBool attackInput;
    /// @brief 장전 여부
    public NetworkBool reloadInput;
    /// @brief 상호작용 여부
    public NetworkBool interactInput;
    /// @brief 조준 방향
    public Vector3 aimForwardVector;
}
