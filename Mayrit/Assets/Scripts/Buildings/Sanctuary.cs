using UnityEngine;

public class Sanctuary : ABuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        _townManager.RegisterSanctuary(this);
    }

    public override void UnregisterBuilding()
    {
        _townManager.UnregisterSanctuary(this);
    }
    #endregion
}
