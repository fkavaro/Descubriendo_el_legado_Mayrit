# Dependency Injection Quick Reference

## Core Pattern

### 1. Service Locator (Already Created)
```csharp
ServiceLocator.Instance.Register<MyManager>(instance);
var manager = ServiceLocator.Instance.Get<MyManager>();
```

### 2. GameInstaller (Already Created)
Place in scene, add all managers, they get auto-registered on Awake.

---

## Converting a Manager Class

### Template
```csharp
// OLD: public class MyManager : Singleton<MyManager>
public class MyManager : MonoBehaviour
{
    // Add dependency fields
    private OtherManager _otherManager;

    void Awake()
    {
        // Inject dependencies
        _otherManager = ServiceLocator.Instance.Get<OtherManager>();
        
        // Validate
        if (_otherManager == null)
            Debug.LogError("MyManager: OtherManager not found!");
    }

    void SomeMethod()
    {
        // OLD: OtherManager.Instance.DoSomething();
        _otherManager.DoSomething();
    }
}
```

---

## Converting a Consumer Class

### Template
```csharp
public class MyClass : MonoBehaviour
{
    private GameManager _gameManager;
    private UIManager _uiManager;

    void Awake()
    {
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
    }

    void Start()
    {
        // Subscribe with null check
        if (_gameManager != null)
            _gameManager.SomeEvent += OnSomeEvent;
    }

    void OnDestroy()
    {
        // Unsubscribe with null check
        if (_gameManager != null)
            _gameManager.SomeEvent -= OnSomeEvent;
    }

    void SomeMethod()
    {
        // OLD: GameManager.Instance.DoSomething();
        _gameManager?.DoSomething();
    }
}
```

---

## Find & Replace Patterns

```csharp
// Pattern 1: Instance access
Find:    ManagerName.Instance
Replace: _managerName

// Pattern 2: ExistingInstance (in cleanup)
Find:    ManagerName.ExistingInstance
Replace: _managerName (with null check)

// Pattern 3: Class declaration
Find:    : Singleton<ClassName>
Replace: : MonoBehaviour

// Pattern 4: Class declaration (BehaviourEntity)
Find:    : ASingletonBehaviourEntity<M, T>
Replace: : MonoBehaviour  // Or keep custom base if needed
```

---

## Common Code Patterns

### Event Subscription
```csharp
// Subscribe
void Start()
{
    if (_manager != null)
        _manager.SomeEvent += Handler;
}

// Unsubscribe
void OnDestroy()
{
    if (_manager != null)
        _manager.SomeEvent -= Handler;
}
```

### Conditional Access
```csharp
// Option 1: Explicit null check
if (_manager != null)
    _manager.DoSomething();

// Option 2: Null conditional operator
_manager?.DoSomething();

// Option 3: Null coalescing for return values
var value = _manager?.GetValue() ?? defaultValue;
```

### Property Access
```csharp
// OLD
var player = GameManager.Instance.PlayableCharacter;

// NEW - Option 1: Direct access
var player = _gameManager.PlayableCharacter;

// NEW - Option 2: Safe access
var player = _gameManager?.PlayableCharacter;

// NEW - Option 3: With null check
PlayableCharacter player = null;
if (_gameManager != null)
    player = _gameManager.PlayableCharacter;
```

---

## Checklist for Each Class

- [ ] Remove Singleton inheritance
- [ ] Add dependency fields
- [ ] Add Awake() method with injections
- [ ] Add null validation for dependencies
- [ ] Replace all `.Instance` calls with fields
- [ ] Replace all `.ExistingInstance` with null-checked fields
- [ ] Update event subscriptions with null checks
- [ ] Update event unsubscriptions with null checks
- [ ] Add to GameInstaller registration
- [ ] Test in Unity Editor

---

## GameInstaller Setup (Per Manager)

```csharp
// 1. Add field
[SerializeField] private MyManager _myManager;

// 2. Register in Awake
if (_myManager != null)
    ServiceLocator.Instance.Register<MyManager>(_myManager);
```

In Unity:
- Select GameInstaller GameObject
- Drag manager GameObject to the field

---

## Execution Order

Set in Unity: Edit → Project Settings → Script Execution Order

Recommended order:
1. ServiceLocator (if needed)
2. GameInstaller (-100)
3. All Managers (0 - default)
4. Game Logic (100+)

---

## Troubleshooting

| Error | Solution |
|-------|----------|
| "Service X not found!" | Add to GameInstaller and assign in Inspector |
| NullReferenceException | Add null check before using service |
| Service not registered | Check GameInstaller.Awake() includes registration |
| Wrong instance | Ensure only one GameInstaller in scene |

---

## Benefits Reminder

✅ Testable - Easy to mock dependencies  
✅ Flexible - Easy to swap implementations  
✅ Clear - Dependencies are explicit  
✅ Maintainable - Easier to refactor  
✅ Debuggable - Less hidden state  

---

## Example Files

- `ServiceLocator.cs` - The service registry
- `GameInstaller.cs` - Registers all services
- `TourManager_DI_Example.cs` - Manager conversion example
- `PlayableCharacter_DI_Example.cs` - Consumer example
- `MIGRATION_STEPS.md` - Detailed step-by-step guide
- `README_DependencyInjection.md` - Full documentation
