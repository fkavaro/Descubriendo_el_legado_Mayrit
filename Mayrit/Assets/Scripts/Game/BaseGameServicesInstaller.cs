using UnityEngine;

public class BaseGameServicesInstaller : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private SoundManager _soundManager;

    protected virtual void Awake()
    {
        // Register all managers as services
        if (_gameManager != null)
            ServiceLocator.Instance.Register(_gameManager);
        else
            Debug.LogError("MainMenuInstaller: GameManager reference is missing!");

        if (_uiManager != null)
            ServiceLocator.Instance.Register(_uiManager);
        else
            Debug.LogError("MainMenuInstaller: UIManager reference is missing!");

        if (_soundManager != null)
            ServiceLocator.Instance.Register(_soundManager);
        else
            Debug.LogError("MainMenuInstaller: SoundManager reference is missing!");
    }

    void OnDestroy()
    {
        ServiceLocator.Instance.Clear();
    }
}
