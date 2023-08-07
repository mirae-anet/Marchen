using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// @brief 보스 광역기 투사체
/// @details 생성 후 0.5초 후에 활성화
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