# Dependency Injection Architecture Diagram

## Before: Singleton Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                        Game Scene                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐                                                │
│  │ GameManager  │ ◄─────── Static Access ────────┐              │
│  │  (Singleton) │                                 │              │
│  └──────────────┘                                 │              │
│         ▲                                         │              │
│         │ Static Access                           │              │
│         │                                         │              │
│  ┌──────────────┐        ┌──────────────┐   ┌────────────┐     │
│  │ UIManager    │        │ TourManager  │   │ Playable   │     │
│  │  (Singleton) │        │  (Singleton) │   │ Character  │     │
│  └──────────────┘        └──────────────┘   └────────────┘     │
│         ▲                       ▲                   │            │
│         └───────────────────────┴───────────────────┘            │
│              Static Instance.Method() calls                      │
│                                                                  │
│  Problems:                                                       │
│  • Tight coupling - direct static dependencies                  │
│  • Hard to test - can't mock singletons easily                  │
│  • Hidden dependencies - not clear what a class needs           │
│  • Global state - accessible from anywhere                      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## After: Dependency Injection Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                        Game Scene                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              ServiceLocator (Registry)                   │   │
│  │  ┌────────┬────────┬────────┬────────────────────────┐ │   │
│  │  │ Game   │ UI     │ Tour   │ Progress │ Camera │ ... │ │   │
│  │  │ Mgr    │ Mgr    │ Mgr    │ Mgr      │ Mgr    │     │ │   │
│  │  └────────┴────────┴────────┴──────────┴────────┴─────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
│         ▲                                                        │
│         │ Registration                                           │
│         │                                                        │
│  ┌──────────────┐                                               │
│  │GameInstaller │  Runs first (Script Execution Order -100)    │
│  │   (Awake)    │  Registers all managers                       │
│  └──────────────┘                                               │
│         │                                                        │
│         │ Registers                                              │
│         ▼                                                        │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │ GameManager  │    │ UIManager    │    │ TourManager  │     │
│  │              │    │              │    │              │     │
│  └──────────────┘    └──────────────┘    └──────────────┘     │
│         │                    │                    │             │
│         └────────────────────┴────────────────────┘             │
│                      Injected into ▼                            │
│                                                                  │
│                         ┌────────────┐                          │
│                         │ Playable   │                          │
│                         │ Character  │                          │
│                         │            │                          │
│                         │ Dependencies:                         │
│                         │ • _gameManager                        │
│                         │ • _uiManager                          │
│                         │ • _tourManager                        │
│                         └────────────┘                          │
│                                                                  │
│  Benefits:                                                       │
│  ✓ Loose coupling - dependencies injected, not hardcoded       │
│  ✓ Easy to test - can inject mocks                             │
│  ✓ Clear dependencies - explicit in Awake()                    │
│  ✓ Better design - forces thinking about architecture          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Execution Flow

```
Scene Start
    │
    ├─ 1. GameInstaller.Awake() executes FIRST (Script Execution Order)
    │     │
    │     ├─ ServiceLocator.Register<GameManager>(gameManager)
    │     ├─ ServiceLocator.Register<UIManager>(uiManager)
    │     ├─ ServiceLocator.Register<TourManager>(tourManager)
    │     ├─ ServiceLocator.Register<ProgressManager>(progressManager)
    │     └─ ... (all other managers)
    │
    ├─ 2. All Manager Awake() methods execute
    │     │
    │     └─ Each manager injects its dependencies from ServiceLocator
    │
    ├─ 3. All other classes' Awake() methods execute
    │     │
    │     └─ Each class injects required manager references
    │
    ├─ 4. Start() methods execute
    │     │
    │     └─ Subscribe to events, initialize systems
    │
    └─ 5. Normal game loop begins
```

## Code Flow Comparison

### Singleton Pattern Flow
```
PlayableCharacter.Start()
    │
    └─> Needs GameManager
        │
        └─> GameManager.Instance  ──┐
                                     │
            Static accessor finds ───┘
            or creates instance
            
            ▼
        GameManager.InputActions.Enable()

Problem: Tight coupling, hidden dependency
```

### Dependency Injection Flow
```
PlayableCharacter.Awake()
    │
    ├─> Needs GameManager
    │   │
    │   └─> ServiceLocator.Get<GameManager>()
    │       │
    │       └─> Returns registered instance
    │
    └─> Stores reference in _gameManager field

PlayableCharacter.Start()
    │
    └─> Uses injected reference
        │
        └─> _gameManager.InputActions.Enable()

Benefit: Explicit dependency, easily testable
```

## File Structure

```
Assets/Scripts/
│
├── DependencyInjection/
│   │
│   ├── ServiceLocator.cs           # Core registry
│   ├── GameInstaller.cs            # Service registration
│   │
│   ├── Examples/
│   │   ├── TourManager_DI_Example.cs
│   │   └── PlayableCharacter_DI_Example.cs
│   │
│   └── Documentation/
│       ├── README_DependencyInjection.md
│       ├── MIGRATION_STEPS.md
│       └── QUICK_REFERENCE.md
│
├── Tours/
│   └── TourManager.cs              # To be migrated
│
├── Game/
│   ├── GameManager.cs              # To be migrated
│   ├── TimeManager.cs              # To be migrated
│   └── TownManager.cs              # To be migrated
│
└── UI/
    └── UIManager.cs                # To be migrated
```

## Testing Example

### Before (Singleton - Hard to Test)
```csharp
[Test]
public void TestPlayableCharacter()
{
    // Problem: Can't control GameManager.Instance
    // It either exists or doesn't, hard to mock
    
    var character = new GameObject().AddComponent<PlayableCharacter>();
    
    // GameManager.Instance is whatever exists in the scene
    // Can't isolate the test
}
```

### After (DI - Easy to Test)
```csharp
[Test]
public void TestPlayableCharacter()
{
    // Create mock manager
    var mockGameManager = new MockGameManager();
    mockGameManager.SetupTestInputActions();
    
    // Register mock in ServiceLocator
    ServiceLocator.Instance.Register<GameManager>(mockGameManager);
    
    // Test with controlled dependencies
    var character = new GameObject().AddComponent<PlayableCharacter>();
    
    // Character uses injected mock, test is isolated ✓
    Assert.IsTrue(character.IsBeingControlled);
}
```

## Key Concepts Summary

| Concept | Description | Example |
|---------|-------------|---------|
| **Service** | A shared component/manager | GameManager, UIManager |
| **Registration** | Adding service to registry | `ServiceLocator.Register<T>(instance)` |
| **Resolution** | Getting service from registry | `ServiceLocator.Get<T>()` |
| **Injection** | Providing dependencies | In `Awake()` method |
| **Consumer** | Class that uses services | PlayableCharacter |
| **Installer** | Registers all services | GameInstaller |

## When to Use Each Pattern

### Use Singleton When:
- ❌ Actually, prefer DI in most cases
- ✓ Very simple utility classes
- ✓ Framework-level services you never mock

### Use Dependency Injection When:
- ✓ Testing is important
- ✓ Multiple implementations possible  
- ✓ Clear architecture needed
- ✓ Reducing coupling is a goal
- ✓ Working in a team
- ✓ **Most Unity projects**
