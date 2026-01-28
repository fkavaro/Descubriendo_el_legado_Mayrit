// using System;
// using UnityEngine;

// public class ServicesInstaller : MonoBehaviour
// {
//     #region EDITOR PROPERTIES
//     [SerializeField] private ServiceConfig<GameManager> _gameManagerConfig;
//     [SerializeField] private ServiceConfig<UIManager> _uiManagerConfig;
//     [SerializeField] private ServiceConfig<SoundManager> _soundManagerConfig;
//     [SerializeField] private ServiceConfig<ProgressManager> _progressManagerConfig;
//     [SerializeField] private ServiceConfig<CameraManager> _cameraManagerConfig;
//     [SerializeField] private ServiceConfig<TourManager> _tourManagerConfig;
//     [SerializeField] private ServiceConfig<TimeManager> _timeManagerConfig;
//     [SerializeField] private ServiceConfig<TownManager> _townManagerConfig;
//     [SerializeField] private ServiceConfig<NPCPoolManager> _npcPoolManagerConfig;
//     #endregion

//     #region LIFE CYCLE
//     protected virtual void Awake()
//     {
//         ServiceLocator.Instance.OnDuplicatedServiceEvent += ReassignDuplicatedService;

//         RegisterInServiceLocator(_gameManagerConfig);
//         RegisterInServiceLocator(_uiManagerConfig);
//         RegisterInServiceLocator(_soundManagerConfig);
//         RegisterInServiceLocator(_progressManagerConfig);
//         RegisterInServiceLocator(_cameraManagerConfig);
//         RegisterInServiceLocator(_tourManagerConfig);
//         RegisterInServiceLocator(_timeManagerConfig);
//         RegisterInServiceLocator(_townManagerConfig);
//         RegisterInServiceLocator(_npcPoolManagerConfig);
//     }

//     protected virtual void OnDestroy()
//     {
//         ServiceLocator.Instance.OnDuplicatedServiceEvent -= ReassignDuplicatedService;

//         // Unregister services that are not marked as DontDestroyOnLoad
//         if (!_gameManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<GameManager>();
//         if (!_uiManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<UIManager>();
//         if (!_soundManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<SoundManager>();
//         if (!_progressManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<ProgressManager>();
//         if (!_cameraManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<CameraManager>();
//         if (!_tourManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<TourManager>();
//         if (!_timeManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<TimeManager>();
//         if (!_townManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<TownManager>();
//         if (!_npcPoolManagerConfig.dontDestroyOnLoad)
//             ServiceLocator.Instance.Unregister<NPCPoolManager>();
//     }
//     #endregion

//     #region HELPERS
//     protected void RegisterInServiceLocator<T>(ServiceConfig<T> serviceConfig) where T : MonoBehaviour
//     {
//         if (serviceConfig.service == null)
//         {
//             Debug.LogError($"{typeof(T)} reference is missing!");
//             return;
//         }

//         ServiceLocator.Instance.Register(serviceConfig);
//     }

//     protected virtual void ReassignDuplicatedService(object obj)
//     {
//         if (obj is GameManager)
//             _gameManagerConfig.service = obj as GameManager;
//         else if (obj is UIManager)
//             _uiManagerConfig.service = obj as UIManager;
//         else if (obj is SoundManager)
//             _soundManagerConfig.service = obj as SoundManager;
//         if (obj is ProgressManager)
//             _progressManagerConfig.service = obj as ProgressManager;
//         else if (obj is CameraManager)
//             _cameraManagerConfig.service = obj as CameraManager;
//         else if (obj is TourManager)
//             _tourManagerConfig.service = obj as TourManager;
//         else if (obj is TimeManager)
//             _timeManagerConfig.service = obj as TimeManager;
//         else if (obj is TownManager)
//             _townManagerConfig.service = obj as TownManager;
//         else if (obj is NPCPoolManager)
//             _npcPoolManagerConfig.service = obj as NPCPoolManager;
//     }
//     #endregion
// }
