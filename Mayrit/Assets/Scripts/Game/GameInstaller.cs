using UnityEngine;

/// <summary>
/// Installer/Bootstrapper that registers all game services at startup.
/// </summary>
public class GameInstaller : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private TourManager _tourManager;
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private TownManager _townManager;
    [SerializeField] private NPCPoolManager _npcPoolManager;

    private void Awake()
    {
        // Register all managers as services
        if (_gameManager != null)
            ServiceLocator.Instance.Register<GameManager>(_gameManager);

        if (_uiManager != null)
            ServiceLocator.Instance.Register<UIManager>(_uiManager);

        if (_progressManager != null)
            ServiceLocator.Instance.Register<ProgressManager>(_progressManager);

        if (_cameraManager != null)
            ServiceLocator.Instance.Register<CameraManager>(_cameraManager);

        if (_tourManager != null)
            ServiceLocator.Instance.Register<TourManager>(_tourManager);

        if (_timeManager != null)
            ServiceLocator.Instance.Register<TimeManager>(_timeManager);

        if (_townManager != null)
            ServiceLocator.Instance.Register<TownManager>(_townManager);

        if (_npcPoolManager != null)
            ServiceLocator.Instance.Register<NPCPoolManager>(_npcPoolManager);

        Debug.Log("All game services registered successfully!");
    }

    private void OnDestroy()
    {
        // Optional: Clear services on destroy
        // ServiceLocator.Instance.Clear();
    }
}
