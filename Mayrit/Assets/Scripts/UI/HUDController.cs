using UnityEngine;
using UnityEngine.UIElements;


public class HUDController : MonoBehaviour
{
    public UIDocument UIDocument;
    Label tooltip;

    void Awake()
    {
        UIDocument = GetComponent<UIDocument>();
        tooltip = UIDocument.rootVisualElement.Q<Label>("Tooltip");

        if (tooltip == null)
        {
            Debug.LogWarning("Tooltip not found");
        }
        else
        {
            tooltip.style.display = DisplayStyle.None; // Hide by default
        }
    }

    void Start()
    {

    }


    void Update()
    {

    }

    public void PlaceTooltip(GameObject gameObject)
    {
        if (tooltip == null) return;

        tooltip.text = gameObject.name;
        Vector3 screen = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        tooltip.style.left = screen.x - (tooltip.layout.width / 2);
        tooltip.style.top = Screen.height - screen.y - 100;

        tooltip.style.display = DisplayStyle.Flex; // Show tooltip
    }

    public void HideTooltip()
    {
        if (tooltip == null) return;

        tooltip.style.display = DisplayStyle.None; // Hide tooltip
    }
}
