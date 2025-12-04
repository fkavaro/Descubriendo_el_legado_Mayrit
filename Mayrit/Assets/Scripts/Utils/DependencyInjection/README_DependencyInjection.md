# Dependency Injection Pattern for Unity

This guide shows how to replace the Singleton pattern with Dependency Injection in your Unity project.

## Overview

**Singleton Pattern** creates global instances that are accessed statically (e.g., `GameManager.Instance`).

**Dependency Injection** provides dependencies to classes through a centralized registry, improving testability and reducing coupling.

## Key Benefits of Dependency Injection

1. **Better Testability**: Easy to mock dependencies for unit tests
2. **Reduced Coupling**: Classes don't depend on concrete implementations
3. **Flexibility**: Easy to swap implementations
4. **Clear Dependencies**: Constructor/Awake injection makes dependencies explicit
5. **No Static State**: Easier to reason about and debug

## Implementation Steps

### Step 1: Create Service Locator

The `ServiceLocator` class acts as a central registry for all services.

```csharp
// Already created in ServiceLocator.cs
```

### Step 2: Create Game Installer

The `GameInstaller` registers all your managers as services at startup.

```csharp
// Already created in GameInstaller.cs
// Attach this to a GameObject in your scene
```

### Step 3: Remove Singleton Inheritance

**Before (Singleton):**
```csharp
public class TourManager : Singleton<TourManager>
{
    void Start()
    {
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
        UIManager.Instance.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
    }
}
```

**After (Dependency Injection):**
```csharp
public class TourManager : MonoBehaviour
{
    private ProgressManager _progressManager;
    private UIManager _uiManager;

    void Awake()
    {
        // Inject dependencies
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
    }

    void Start()
    {
        // Use injected dependencies
        _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
        _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
    }
}
```

### Step 4: Update All References

Replace all `ManagerName.Instance` calls with injected references.

**Before:**
```csharp
if (GameManager.Instance.PlayableCharacter.IsBeingControlled)
{
    GameManager.Instance.InputActions.Player.Enable();
}
```

**After:**
```csharp
if (_gameManager.PlayableCharacter.IsBeingControlled)
{
    _gameManager.InputActions.Player.Enable();
}
```

## Scene Setup

1. Create an empty GameObject named "GameInstaller" in your scene
2. Attach the `GameInstaller` component to it
3. Drag and drop all your manager GameObjects into the appropriate fields
4. Ensure GameInstaller executes before other managers (Script Execution Order)

## Migration Checklist

For each Singleton class:

- [ ] Remove `: Singleton<T>` or `: ASingletonBehaviourEntity<T, U>` inheritance
- [ ] Change to `: MonoBehaviour` (or keep existing base class if needed)
- [ ] Add private fields for dependencies
- [ ] Inject dependencies in `Awake()` using `ServiceLocator.Instance.Get<T>()`
- [ ] Add null checks for injected dependencies
- [ ] Replace all `Manager.Instance` calls with private field references
- [ ] Update event subscriptions/unsubscriptions
- [ ] Add the manager to `GameInstaller`
- [ ] Test thoroughly

## Advanced: Constructor Injection

For non-MonoBehaviour classes, use constructor injection:

```csharp
public class PathVisualizer
{
    private readonly ProgressManager _progressManager;

    // Constructor injection
    public PathVisualizer(ProgressManager progressManager, /* other params */)
    {
        _progressManager = progressManager;
        // ...
    }
}
```

## Testing Benefits

With DI, you can easily create mock services for testing:

```csharp
[Test]
public void TestTourManager()
{
    // Create mock services
    var mockProgressManager = new MockProgressManager();
    var mockUIManager = new MockUIManager();
    
    // Register mocks
    ServiceLocator.Instance.Register<ProgressManager>(mockProgressManager);
    ServiceLocator.Instance.Register<UIManager>(mockUIManager);
    
    // Test your class
    var tourManager = new GameObject().AddComponent<TourManager>();
    // Assert expected behavior...
}
```

## Common Patterns

### Pattern 1: Optional Dependencies
```csharp
void Awake()
{
    _optionalService = ServiceLocator.Instance.Get<OptionalService>();
    // Don't log error if optional
}

void Update()
{
    if (_optionalService != null)
    {
        _optionalService.DoSomething();
    }
}
```

### Pattern 2: Late Binding
```csharp
private GameManager GameManager => 
    _gameManager ?? (_gameManager = ServiceLocator.Instance.Get<GameManager>());
```

### Pattern 3: Manual Registration
```csharp
void Awake()
{
    // Self-register if not using GameInstaller
    ServiceLocator.Instance.Register<MyManager>(this);
}
```

## Comparison Table

| Aspect | Singleton | Dependency Injection |
|--------|-----------|---------------------|
| Access | `Manager.Instance.Method()` | `_manager.Method()` |
| Testability | Difficult | Easy |
| Dependencies | Hidden | Explicit |
| Coupling | High | Low |
| Flexibility | Low | High |
| Setup | Automatic | Manual (via Installer) |

## Migration Example Files

See the example implementations:
- `TourManager_DI_Example.cs` - Full manager conversion
- `PlayableCharacter_DI_Example.cs` - Consumer class pattern
- `GameInstaller.cs` - Service registration

## Notes

- ServiceLocator itself is still a singleton, but it's a controlled one
- For more advanced DI, consider libraries like Zenject/Extenject or VContainer
- Keep the ServiceLocator pattern simple for Unity projects
- Consider Script Execution Order to ensure installers run first
