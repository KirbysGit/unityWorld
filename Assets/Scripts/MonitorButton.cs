using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] Key buttonKey = Key.E;  // Key to press when hovering
    [SerializeField] string buttonPrompt = "Press E to turn on monitor";
    
    [Header("Visual Feedback")]
    [SerializeField] GameObject buttonLight;         // Optional light to show button state
    [SerializeField] Material onMaterial;            // Material when monitor is on
    [SerializeField] Material offMaterial;           // Material when monitor is off
    [SerializeField] Material hoverMaterial;         // Material when hovering over button
    
    [Header("References")]
    [SerializeField] MonitorController monitorController;  // The monitor this button controls
    
    private bool isHovering = false;
    private bool isMonitorOn = false;
    private Renderer buttonRenderer;
    private Material originalMaterial;
    
    void Start()
    {
        // Find monitor controller if not assigned
        if (monitorController == null)
        {
            monitorController = FindFirstObjectByType<MonitorController>();
        }
        
        // Get button renderer and store original material
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            originalMaterial = buttonRenderer.material;
        }
        
        UpdateButtonVisuals();
    }
    
    void Update()
    {
        // Handle button press when hovering
        if (isHovering && Keyboard.current[buttonKey].wasPressedThisFrame)
        {
            ToggleMonitor();
        }
    }
    
    void OnMouseEnter()
    {
        isHovering = true;
        UpdateHoverVisuals();
    }
    
    void OnMouseExit()
    {
        isHovering = false;
        UpdateHoverVisuals();
    }
    
    void ToggleMonitor()
    {
        isMonitorOn = !isMonitorOn;
        
        if (monitorController != null)
        {
            monitorController.SetMonitorOn(isMonitorOn);
        }
        
        UpdateButtonVisuals();
        
        Debug.Log($"Monitor turned {(isMonitorOn ? "ON" : "OFF")}");
    }
    
    void UpdateButtonVisuals()
    {
        // Update button light
        if (buttonLight != null)
        {
            buttonLight.SetActive(isMonitorOn);
        }
        
        // Update button material (only if not hovering)
        if (buttonRenderer != null && !isHovering)
        {
            buttonRenderer.material = isMonitorOn ? onMaterial : offMaterial;
        }
    }
    
    void UpdateHoverVisuals()
    {
        if (buttonRenderer != null)
        {
            if (isHovering && hoverMaterial != null)
            {
                buttonRenderer.material = hoverMaterial;
            }
            else
            {
                // Return to normal state material
                buttonRenderer.material = isMonitorOn ? onMaterial : offMaterial;
            }
        }
    }
    
    void OnGUI()
    {
        // Show interaction prompt when hovering
        if (isHovering)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 50, 200, 20), buttonPrompt);
        }
    }
    
    // Method to set monitor state from MonitorFocus
    public void SetMonitorState(bool on)
    {
        isMonitorOn = on;
        UpdateButtonVisuals();
    }
}
