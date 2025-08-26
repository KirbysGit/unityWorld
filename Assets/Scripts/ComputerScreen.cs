using UnityEngine;
using UnityEngine.UI;

namespace Seagull.Interior_01 {
    public class ComputerScreen : MonoBehaviour {
        [Header("Screen Setup")]
        [SerializeField] private GameObject screenQuad;
        [SerializeField] private Camera screenCamera;
        [SerializeField] private RenderTexture screenRenderTexture;
        
        [Header("Desktop Settings")]
        [SerializeField] private Canvas desktopCanvas;
        [SerializeField] private int screenWidth = 1920;
        [SerializeField] private int screenHeight = 1080;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private Material screenMaterial;
        private bool isScreenActive = false;
        
        void Start() {
            InitializeScreen();
        }
        
        void InitializeScreen() {
            // Create render texture if it doesn't exist
            if (screenRenderTexture == null) {
                screenRenderTexture = new RenderTexture(screenWidth, screenHeight, 24);
                screenRenderTexture.name = "ComputerScreen_RenderTexture";
            }
            
            // Get or create screen camera
            if (screenCamera == null) {
                screenCamera = GetComponentInChildren<Camera>();
                if (screenCamera == null) {
                    GameObject cameraObj = new GameObject("ScreenCamera");
                    cameraObj.transform.SetParent(transform);
                    cameraObj.transform.localPosition = Vector3.forward * 0.1f;
                    screenCamera = cameraObj.AddComponent<Camera>();
                }
            }
            
            // Configure screen camera
            ConfigureScreenCamera();
            
            // Setup screen material
            SetupScreenMaterial();
            
            // Setup desktop canvas
            SetupDesktopCanvas();
            
            if (showDebugInfo) {
                Debug.Log("ComputerScreen initialized successfully");
            }
        }
        
        void ConfigureScreenCamera() {
            screenCamera.clearFlags = CameraClearFlags.SolidColor;
            screenCamera.backgroundColor = Color.black;
            screenCamera.cullingMask = LayerMask.GetMask("UI"); // Only render UI layer
            screenCamera.targetTexture = screenRenderTexture;
            screenCamera.orthographic = true;
            screenCamera.orthographicSize = 5f;
            screenCamera.depth = 1; // Render after main camera
        }
        
        void SetupScreenMaterial() {
            if (screenQuad != null) {
                screenMaterial = screenQuad.GetComponent<MeshRenderer>().material;
                screenMaterial.mainTexture = screenRenderTexture;
            }
        }
        
        void SetupDesktopCanvas() {
            if (desktopCanvas == null) {
                // Create desktop canvas
                GameObject canvasObj = new GameObject("DesktopCanvas");
                canvasObj.transform.SetParent(screenCamera.transform);
                canvasObj.transform.localPosition = Vector3.forward * 0.01f;
                
                desktopCanvas = canvasObj.AddComponent<Canvas>();
                desktopCanvas.renderMode = RenderMode.WorldSpace;
                desktopCanvas.worldCamera = screenCamera;
                
                // Add canvas scaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(screenWidth, screenHeight);
                
                // Add graphic raycaster for input
                canvasObj.AddComponent<GraphicRaycaster>();
                
                if (showDebugInfo) {
                    Debug.Log("Desktop canvas created and configured");
                }
            }
        }
        
        public void TurnOnScreen() {
            if (screenQuad != null) {
                screenQuad.SetActive(true);
                isScreenActive = true;
                
                if (showDebugInfo) {
                    Debug.Log("Computer screen turned ON");
                }
            }
        }
        
        public void TurnOffScreen() {
            if (screenQuad != null) {
                screenQuad.SetActive(false);
                isScreenActive = false;
                
                if (showDebugInfo) {
                    Debug.Log("Computer screen turned OFF");
                }
            }
        }
        
        public bool IsScreenActive() {
            return isScreenActive;
        }
        
        public Canvas GetDesktopCanvas() {
            return desktopCanvas;
        }
        
        void OnDestroy() {
            if (screenRenderTexture != null) {
                screenRenderTexture.Release();
            }
        }
    }
}
