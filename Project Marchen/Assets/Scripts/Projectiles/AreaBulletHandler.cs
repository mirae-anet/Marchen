using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AreaBulletHandler : BulletHandler
{
    TickTimer activeDelayTimer = TickTimer.None;

    public override void Fire(PlayerRef firedByPlayerRef, NetworkObject firedByNetworkObject, string firedByName)
    {
        base.Fire(firedByPlayerRef, firedByNetworkObject, firedByName);
        activeDelayTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
    }

    protected override void CheckForImpactPoint()
    {
        if(activeDelayTimer.Expired(Runner))
            base.CheckForImpactPoint();
    }
}