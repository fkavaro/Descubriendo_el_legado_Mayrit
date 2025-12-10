using UnityEngine;

/// <summary>
/// Installer/Bootstrapper that registers all game services at startup.
/// </summary>
public class GamePlayServicesInstaller : BaseGameServicesInstaller
{
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private TourManager _tourManager;
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private TownManager _townManager;
    [SerializeField] private NPCPoolManager _npcPoolManager;

    protected override void Awake()
    {
        base.Awake();

        if (_progressManager != null)
            ServiceLocator.Instance.Register(_progressManager);
        else
            Debug.LogError("GamePlayInstaller: ProgressManager reference is missing!");

        if (_cameraManager != null)
            ServiceLocator.Instance.Register(_cameraManager);
        else
            Debug.LogError("GamePlayInstaller: CameraManager reference is missing!");

        if (_tourManager != null)
            ServiceLocator.Instance.Register(_tourManager);
        else
            Debug.LogError("GamePlayInstaller: TourManager reference is missing!");

        if (_timeManager != null)
            ServiceLocator.Instance.Register(_timeManager);
        else
            Debug.LogError("GamePlayInstaller: TimeManager reference is missing!");

        if (_townManager != null)
            ServiceLocator.Instance.Register(_townManager);
        else
            Debug.LogError("GamePlayInstaller: TownManager reference is missing!");

        if (_npcPoolManager != null)
            ServiceLocator.Instance.Register(_npcPoolManager);
        else
            Debug.LogError("GamePlayInstaller: NPCPoolManager reference is missing!");
    }
}
