using UnityEngine;

public class Sanctuary : ABuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        TownManager.RegisterSanctuary(this);
    }

    public override void UnregisterBuilding()
    {
        TownManager.UnregisterSanctuary(this);
    }
    #endregion
}
