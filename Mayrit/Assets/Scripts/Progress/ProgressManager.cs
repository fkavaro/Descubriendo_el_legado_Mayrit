using UnityEngine;

public class ProgressManager : Singleton<ProgressManager>
{
    #region PUBLIC PROPERTIES
    public EventInformationSO _currentEvent;

    // Finite State Machine
    public FiniteStateMachine<ProgressManager> _fsm;

    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED
    protected override void OnAwake()
    {

    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }
    protected override ADecisionSystem<ProgressManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS

    #endregion
}
