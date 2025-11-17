using UnityEngine;

public class Sanctuary : ABuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.RegisterSanctuary(this);
    }

    public override void UnregisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.UnregisterSanctuary(this);
    }
    #endregion
}
