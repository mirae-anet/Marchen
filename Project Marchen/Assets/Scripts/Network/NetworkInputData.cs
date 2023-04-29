using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 lookForwardVector;
    public Vector3 aimForwardVector;
    public NetworkBool isJumpButtonPressed;

    public NetworkBool isFireButtonPressed;
    public NetworkBool isGrenadeFireButtonPressed;
    public NetworkBool isRocketLauncherFireButtonPressed;
}
