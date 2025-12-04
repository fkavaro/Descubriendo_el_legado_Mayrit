# Step-by-Step Migration Guide: Singleton to Dependency Injection

This guide shows exactly how to convert a class from Singleton pattern to Dependency Injection.

## Example: Converting TourManager

### STEP 1: Change the Class Declaration

**Before:**
```csharp
public class TourManager : Singleton<TourManager>
```

**After:**
```csharp
public class TourManager : MonoBehaviour
```

---

### STEP 2: Add Dependency Fields

Add private fields at the top of your class for each manager you need:

```csharp
public class TourManager : MonoBehaviour
{
    // Injected dependencies
    private ProgressManager _progressManager;
    private UIManager _uiManager;
    private GameManager _gameManager;
    
    // ... rest of your existing fields
```

---

### STEP 3: Inject Dependencies in Awake()

Create or update your `Awake()` method to get the services:

```csharp
void Awake()
{
    // Get services from ServiceLocator
    _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
    _uiManager = ServiceLocator.Instance.Get<UIManager>();
    _gameManager = ServiceLocator.Instance.Get<GameManager>();

    // Optional but recommended: Validate dependencies
    if (_progressManager == null)
        Debug.LogError("TourManager: ProgressManager not found!");
    if (_uiManager == null)
        Debug.LogError("TourManager: UIManager not found!");
    if (_gameManager == null)
        Debug.LogError("TourManager: GameManager not found!");
}
```

---

### STEP 4: Replace All Singleton.Instance Calls

Find and replace every `ManagerName.Instance` with the injected field:

**Before:**
```csharp
void Start()
{
    ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
    UIManager.Instance.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
}

void Update()
{
    if (GameManager.Instance.PlayableCharacter.IsBeingControlled)
    {
        // Do something
    }
}
```

**After:**
```csharp
void Start()
{
    _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
    _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
}

void Update()
{
    if (_gameManager.PlayableCharacter.IsBeingControlled)
    {
        // Do something
    }
}
```

---

### STEP 5: Update Event Unsubscriptions

Add null checks when unsubscribing from events:

**Before:**
```csharp
void OnDisable()
{
    ProgressManager.ExistingInstance.OnMilestoneChangedEvent -= OnMilestoneChanged;
    UIManager.ExistingInstance.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
}
```

**After:**
```csharp
void OnDisable()
{
    if (_progressManager != null)
        _progressManager.OnMilestoneChangedEvent -= OnMilestoneChanged;
    if (_uiManager != null)
        _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
}
```

---

### STEP 6: Register in GameInstaller

Add your manager to `GameInstaller.cs`:

```csharp
public class GameInstaller : MonoBehaviour
{
    [SerializeField] private TourManager _tourManager;  // Add this line
    // ... other managers

    private void Awake()
    {
        if (_tourManager != null)
            ServiceLocator.Instance.Register<TourManager>(_tourManager);
        // ... other registrations
    }
}
```

Then in Unity Editor:
1. Select the GameObject with GameInstaller component
2. Drag your TourManager GameObject into the new field

---

### STEP 7: Update Consumer Classes

Any other class that uses this manager needs to be updated too:

**Before:**
```csharp
public class PlayableCharacter : MonoBehaviour
{
    void Start()
    {
        TourManager.Instance.TourPOIVisitedEvent += OnTourPOIVisited;
    }
}
```

**After:**
```csharp
public class PlayableCharacter : MonoBehaviour
{
    private TourManager _tourManager;

    void Awake()
    {
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
    }

    void Start()
    {
        if (_tourManager != null)
            _tourManager.TourPOIVisitedEvent += OnTourPOIVisited;
    }
}
```

---

## Complete Before/After Example

### BEFORE (Singleton Pattern)

```csharp
public class TourManager : Singleton<TourManager>
{
    [SerializeField] Tour _currentTour;
    PathVisualizer _pathVisualizer;

    void Start()
    {
        _pathVisualizer = new PathVisualizer(GetComponent<LineRenderer>());
        
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
        UIManager.Instance.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
    }

    void Update()
    {
        if (GameManager.Instance.PlayableCharacter.IsBeingControlled &&
            _currentTour != null && !_currentTour.IsCompleted)
        {
            _pathVisualizer.UpdatePath();
        }
    }

    void OnDisable()
    {
        ProgressManager.ExistingInstance.OnMilestoneChangedEvent -= OnMilestoneChanged;
        UIManager.ExistingInstance.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
    }

    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        AttachToTour(milestoneMapping?.Tour);
    }
}
```

### AFTER (Dependency Injection)

```csharp
public class TourManager : MonoBehaviour
{
    [SerializeField] Tour _currentTour;
    PathVisualizer _pathVisualizer;

    // Injected dependencies
    private ProgressManager _progressManager;
    private UIManager _uiManager;
    private GameManager _gameManager;

    void Awake()
    {
        // Inject dependencies
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        // Validate
        if (_progressManager == null)
            Debug.LogError("TourManager: ProgressManager not found!");
        if (_uiManager == null)
            Debug.LogError("TourManager: UIManager not found!");
        if (_gameManager == null)
            Debug.LogError("TourManager: GameManager not found!");
    }

    void Start()
    {
        _pathVisualizer = new PathVisualizer(GetComponent<LineRenderer>());
        
        if (_progressManager != null)
            _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
        if (_uiManager != null)
            _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
    }

    void Update()
    {
        if (_gameManager != null && 
            _gameManager.PlayableCharacter != null &&
            _gameManager.PlayableCharacter.IsBeingControlled &&
            _currentTour != null && !_currentTour.IsCompleted)
        {
            _pathVisualizer.UpdatePath();
        }
    }

    void OnDisable()
    {
        if (_progressManager != null)
            _progressManager.OnMilestoneChangedEvent -= OnMilestoneChanged;
        if (_uiManager != null)
            _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
    }

    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        AttachToTour(milestoneMapping?.Tour);
    }
}
```

---

## Quick Find & Replace

Use these patterns to help with migration:

| Find | Replace |
|------|---------|
| `ManagerName.Instance` | `_managerName` |
| `ManagerName.ExistingInstance` | `_managerName` (with null check) |
| `: Singleton<ManagerName>` | `: MonoBehaviour` |
| `: ASingletonBehaviourEntity<M, T>` | `: MonoBehaviour` (adjust based on your needs) |

---

## Common Pitfalls

1. **Forgetting to register in GameInstaller** - Your services won't be found
2. **Order of execution** - Ensure GameInstaller runs before other scripts (Script Execution Order in Unity)
3. **Missing null checks** - Always check if injected services are not null before using
4. **Circular dependencies** - If A needs B and B needs A, you have a design problem
5. **Not updating all references** - Use Find in Files to locate all `ManagerName.Instance` calls

---

## Testing Your Migration

After migrating a class:

1. ✅ Check Unity Console for "not found" errors
2. ✅ Test all functionality that uses the migrated class
3. ✅ Verify event subscriptions work correctly
4. ✅ Check scene transitions still work
5. ✅ Run your existing tests (if any)

---

## Need Help?

- Review `TourManager_DI_Example.cs` for a complete working example
- Review `PlayableCharacter_DI_Example.cs` for consumer pattern
- See `README_DependencyInjection.md` for more details
