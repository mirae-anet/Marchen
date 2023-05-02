using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    // public Vector3 lookForwardVector; //old
    // public Vector2 movementInput;
    public Vector3 moveDir;
    public NetworkBool isMove;
    // public NetworkBool isJump;
    // public NetworkBool isDodge;
    public Vector3 aimForwardVector;
    public NetworkBool isJumpButtonPressed;

    public NetworkBool isFireButtonPressed;
    public NetworkBool isGrenadeFireButtonPressed;
    public NetworkBool isRocketLauncherFireButtonPressed;
}
