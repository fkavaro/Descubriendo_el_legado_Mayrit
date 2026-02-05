using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProgressManager : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public Milestone_DataSO CurrentMilestoneMapping => _milestoneMappings[_currentMilestoneIndex];

    public int CurrentMilestoneIndex => _currentMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Debug tweaks")]
    [SerializeField] bool _canSkipTours = false;

    // TODO: remove eventually
    // [Tooltip("Wether to update scene at milestone changes in editor")]
    // [SerializeField] bool _updateInEditor = false;
    [Header("Milestones")]
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] Milestone_DataSO _currentMilestoneMapping;
    [SerializeField] List<Milestone_DataSO> _milestoneMappings = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Milestone_DataSO> MilestoneChangedEvent;

    // TODO: remove eventually
    // public event Action<bool> OnEditorUpdateChangedEvent;
    // int _lastValidatedMilestoneIndex;
    // bool _lastUpdateInEditor;

    FiniteStateMachine<MilestoneState> _fsm;

    ScenesController _scenesController;
    TourManager _tourManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        // Build state for each milestone scene
        for (int i = 0; i < _milestoneMappings.Count; i++)
            _fsm.AddStateToSequence(new MilestoneState(_milestoneMappings[i]));

        _fsm.SwitchedStateEvent += OnStateSwitch;

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    // TODO remove eventually
    //     // Called when the script is loaded or a value is changed in the inspector
    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;

    //         // Invoke milestone changed event when _currentMilestoneIndex is changed in inspector
    //         // and _updateInInspector is true
    //         if (_lastValidatedMilestoneIndex != _currentMilestoneIndex)
    //         {
    //             _lastValidatedMilestoneIndex = _currentMilestoneIndex;

    //             if (_updateInEditor)
    //             {
    //                 _currentMilestoneMapping = _milestoneMappings[_currentMilestoneIndex];
    //                 // To avoid issues with re-entrancy
    //                 UnityEditor.EditorApplication.delayCall += () => MilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    //             }
    //         }

    //         // Invoke editor update changed event when _updateInEditor is changed in inspector
    //         if (_lastUpdateInEditor != _updateInEditor)
    //         {
    //             _lastUpdateInEditor = _updateInEditor;

    //             if (_updateInEditor)
    //                 _currentMilestoneMapping = _milestoneMappings[_currentMilestoneIndex];
    //             else
    //                 _currentMilestoneMapping = null;

    //             UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChangedEvent?.Invoke(_updateInEditor);
    //         }
    // #endif
    //     }

    protected override void Awake()
    {
        ServiceLocator.Instance.Register(this);

        base.Awake();
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();

        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;
    }

    void OnDisable()
    {
        _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToNextMilestone() => _fsm.SwitchToNextStateInSequence();
    public void SwitchToPreviousMilestone() => _fsm.SwitchToPreviousStateInSequence();
    public bool AtFirstMilestone() => _fsm.AtFistStateInSequence();
    public bool AtLastMilestone() => _fsm.AtLastStateInSequence();
    #endregion

    #region PRIVATE METHODS
    // TODO: remove eventually
    // MilestoneMapping GetMappingForIndex(int index)
    // {
    //     if (_milestonesMappings == null)
    //         BuildMappingCache();

    //     if (_milestonesMappings.TryGetValue(index, out MilestoneMapping mapping))
    //         return mapping;

    //     return null;
    // }

    public bool IsNextMilestoneAvailable()
    {
        if (_tourManager.CurrentTour == null)
        {
            if (DebugMode)
                Debug.LogWarning("ProgressManager: No current tour found. Next milestone availability will always return false.");
            return false;
        }


        bool canSkipInRuntime = _canSkipTours; //Application.isPlaying && Application.isEditor // TODO: full line when gold release
        bool tourCompleted = _tourManager.CurrentTour.IsCompleted;
        bool isNextMilestoneAvailable = !AtLastMilestone() && (canSkipInRuntime || tourCompleted);

        return isNextMilestoneAvailable;
    }
    #endregion

    #region CALLBACK METHODS
    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
            base.Start(); // Start behaviour system after scene is fully loaded
    }

    void OnStateSwitch()
    {
        _currentMilestoneMapping = _fsm.CurrentState.Data;
        _currentMilestoneIndex = _currentMilestoneMapping.Index;
        MilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    }
    #endregion
}
