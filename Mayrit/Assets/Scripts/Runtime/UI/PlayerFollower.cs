using UnityEngine;
using UnityEngine.UIElements;

public class PlayerFollower
{
    #region PROPERTIES
    const float NEAR_ZERO = 0.0001f;

    private readonly VisualElement _container;
    private readonly VisualElement _angle;
    UIManager _uiManager;
    CameraManager _cameraManager;

    public Transform PlayerTransform;
    Vector2 _screenCenter;
    Vector2 _followerCenter;
    Vector2 _direction;
    Vector2 _clampedScreenPos;
    Vector3 _playerScreenPos;
    Vector3 _playerViewportPos;
    #endregion

    #region CONSTRUCTOR
    public PlayerFollower(VisualElement container)
    {
        _container = container;
        _angle = _container.Q<VisualElement>("Angle");

        if (_angle == null)
            Debug.LogWarning("PlayerFollower: No VisualElement with name 'Angle' was found in the container.");
    }
    #endregion

    #region PUBLIC METHODS
    public void Start()
    {
        if (_uiManager == null)
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
        if (_cameraManager == null)
            _cameraManager = ServiceLocator.Instance.Get<CameraManager>();

        if (_uiManager == null)
            Debug.LogWarning("PlayerFollower: UIManager not found in ServiceLocator.");
        if (_cameraManager == null)
            Debug.LogWarning("PlayerFollower: CameraManager not found in ServiceLocator.");
    }

    public void Update()
    {
        if (PlayerTransform == null ||
            !_cameraManager.IsInSpectatorState ||
            Camera.main == null)
        {
            HideFollower();
            return;
        }

        Camera mainCamera = Camera.main; // TODO: avoid this

        // Project the player into screen/viewport space for visibility checks.
        _playerScreenPos = mainCamera.WorldToScreenPoint(PlayerTransform.position);
        _playerViewportPos = mainCamera.WorldToViewportPoint(PlayerTransform.position);

        if (IsOnScreen(_playerScreenPos, _playerViewportPos))
        {
            HideFollower();
            return;
        }

        ShowFollower();

        _screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        _followerCenter = GetCenter(_container);

        // Direction from screen center to player (flipped if behind camera).
        _direction = GetDirectionFromCenter(_playerScreenPos, _screenCenter);

        // Place the follower on the screen border along that direction.
        _clampedScreenPos = ClampToScreenBorder(_direction, _followerCenter);
        SetFollowerPosition(_clampedScreenPos, _followerCenter);
        SetFollowerRotation(_direction);
    }
    #endregion

    #region PRIVATE METHODS
    void HideFollower()
    {
        if (_container.style.display != DisplayStyle.None)
            _container.style.display = DisplayStyle.None;
    }

    void ShowFollower()
    {
        if (_container.style.display != DisplayStyle.Flex)
            _container.style.display = DisplayStyle.Flex;
    }

    Vector2 ScreenToUICoordinates(Vector3 screenPos)
    {
        // Convert from screen space (Y=0 at bottom, Y up) to UI space (Y=0 at top, Y down)
        return new Vector2(screenPos.x, Screen.height - screenPos.y);
    }

    bool IsOnScreen(Vector3 screenPos, Vector3 viewportPos)
    {
        // Screen-space Z > 0 means in front of camera; viewport in [0,1] means visible.
        return screenPos.z > 0f
            && viewportPos.x >= 0f && viewportPos.x <= 1f
            && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    Vector2 GetDirectionFromCenter(Vector3 screenPos, Vector2 screenCenter)
    {
        // Convert both positions to UI space for consistent direction calculation
        Vector2 playerUIPos = ScreenToUICoordinates(screenPos);
        Vector2 centerUIPos = ScreenToUICoordinates(screenCenter);

        Vector2 direction = playerUIPos - centerUIPos;

        // If behind camera, flip so the arrow still points toward the target.
        if (screenPos.z < 0f)
            direction = -direction;

        // Avoid unstable direction when target is centered.
        if (direction.sqrMagnitude < NEAR_ZERO)
            direction = Vector2.up;

        return direction.normalized;
    }

    Vector2 GetCenter(VisualElement visualElement)
    {
        // Use resolved size and keep a minimum so the element stays on-screen.
        float width = visualElement.resolvedStyle.width;
        float height = visualElement.resolvedStyle.height;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        return new Vector2(halfWidth, halfHeight);
    }

    Vector2 ClampToScreenBorder(Vector2 direction, Vector2 halfSize)
    {
        // Convert screen center to UI coordinates for clamping
        Vector2 uiScreenCenter = ScreenToUICoordinates(_screenCenter);

        // Intersect the ray from the center with the screen rectangle in UI space.
        float minX = halfSize.x + _uiManager.PlayerFollowerScreenMargin.x;
        float maxX = Screen.width - halfSize.x - _uiManager.PlayerFollowerScreenMargin.x;
        float minY = halfSize.y + _uiManager.PlayerFollowerScreenMargin.y;
        float maxY = Screen.height - halfSize.y - _uiManager.PlayerFollowerScreenMargin.y;

        float tx = Mathf.Abs(direction.x) < NEAR_ZERO
            ? float.PositiveInfinity
            : (direction.x > 0f ? (maxX - uiScreenCenter.x) / direction.x : (minX - uiScreenCenter.x) / direction.x);
        float ty = Mathf.Abs(direction.y) < NEAR_ZERO
            ? float.PositiveInfinity
            : (direction.y > 0f ? (maxY - uiScreenCenter.y) / direction.y : (minY - uiScreenCenter.y) / direction.y);

        float t = Mathf.Min(tx, ty);
        return uiScreenCenter + direction * t;
    }

    void SetFollowerPosition(Vector2 screenPos, Vector2 halfSize)
    {
        // screenPos is already in UI Toolkit coordinates (from ClampToScreenBorder)
        // Convert from center to top-left corner
        float uiX = screenPos.x - halfSize.x - _uiManager.PlayerFollowerPositionOffset.x;
        float uiY = screenPos.y - halfSize.y - _uiManager.PlayerFollowerPositionOffset.y;

        _container.style.left = uiX;
        _container.style.top = uiY;
    }

    void SetFollowerRotation(Vector2 direction)
    {
        // direction is in UI space (Y down), and Atan2 works correctly for this
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        _angle.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
    }
    #endregion
}
