using UnityEngine;

public class MonitorController : MonoBehaviour
{
    [Header("Monitor Components")]
    [SerializeField] MonitorCamLook cameraController;  // The camera look script
    [SerializeField] GameObject monitorScreen;         // The monitor screen object
    
    [Header("Screen Materials")]
    [SerializeField] Material screenOnMaterial;        // Material when monitor is on
    [SerializeField] Material screenOffMaterial;       // Material when monitor is off
    
    private bool isMonitorOn = false;
    private Renderer screenRenderer;
    
    void Start()
    {
        // Get screen renderer
        if (monitorScreen != null)
        {
            screenRenderer = monitorScreen.GetComponent<Renderer>();
        }
        
        // Find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = GetComponent<MonitorCamLook>();
        }
        
        // Initialize monitor as off
        SetMonitorOn(false);
    }
    
    public void SetMonitorOn(bool on)
    {
        isMonitorOn = on;
        
        // Enable/disable camera controls
        if (cameraController != null)
        {
            cameraController.SetAllowLook(on);
        }
        
        // Update screen appearance
        UpdateScreenAppearance();
        
        Debug.Log($"Monitor Controller: Monitor turned {(on ? "ON" : "OFF")}");
    }
    
    void UpdateScreenAppearance()
    {
        if (screenRenderer != null)
        {
            if (isMonitorOn && screenOnMaterial != null)
            {
                screenRenderer.material = screenOnMaterial;
            }
            else if (!isMonitorOn && screenOffMaterial != null)
            {
                screenRenderer.material = screenOffMaterial;
            }
        }
    }
    
    public bool IsMonitorOn()
    {
        return isMonitorOn;
    }
}
