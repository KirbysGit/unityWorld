using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Seagull.Interior_01 {
    public class ComputerInteraction : MonoBehaviour {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask playerLayer = 1;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Header("Camera System")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Camera monitorCamera;
        [SerializeField] private Transform sittingPosition;
        [SerializeField] private float transitionSpeed = 2f;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private GameObject sittingUI;
        [SerializeField] private Button sitDownButton;
        [SerializeField] private Button standUpButton;
        
        [Header("Player Control")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private MonoBehaviour[] playerScriptsToDisable;
        
        private bool playerInRange = false;
        private bool isSitting = false;
        private bool isTransitioning = false;
        private Vector3 originalPlayerPosition;
        private Quaternion originalPlayerRotation;
        private Camera originalCamera;
        
        void Start() {
            InitializeUI();
            SetupButtons();
            
            // Find player camera if not assigned
            if (playerCamera == null) {
                playerCamera = Camera.main;
            }
            
            // Find player if not assigned
            if (playerObject == null) {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
        }
        
        void Update() {
            CheckPlayerProximity();
            HandleInput();
        }
        
        void CheckPlayerProximity() {
            if (playerObject == null) return;
            
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            // Show/hide interaction prompt
            if (playerInRange && !wasInRange && !isSitting) {
                ShowInteractionPrompt();
            } else if (!playerInRange && wasInRange && !isSitting) {
                HideInteractionPrompt();
            }
        }
        
        void HandleInput() {
            if (playerInRange && !isSitting && Input.GetKeyDown(interactKey)) {
                ShowSittingOptions();
            }
        }
        
        void InitializeUI() {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(false);
            }
            
            if (sittingUI != null) {
                sittingUI.SetActive(false);
            }
        }
        
        void SetupButtons() {
            if (sitDownButton != null) {
                sitDownButton.onClick.AddListener(SitDown);
            }
            
            if (standUpButton != null) {
                standUpButton.onClick.AddListener(StandUp);
            }
        }
        
        void ShowInteractionPrompt() {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(true);
            }
        }
        
        void HideInteractionPrompt() {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(false);
            }
        }
        
        void ShowSittingOptions() {
            HideInteractionPrompt();
            if (sittingUI != null) {
                sittingUI.SetActive(true);
            }
        }
        
        public void SitDown() {
            if (isTransitioning) return;
            
            StartCoroutine(TransitionToSitting());
        }
        
        public void StandUp() {
            if (isTransitioning) return;
            
            StartCoroutine(TransitionToStanding());
        }
        
        System.Collections.IEnumerator TransitionToSitting() {
            isTransitioning = true;
            
            // Store original player state
            originalPlayerPosition = playerObject.transform.position;
            originalPlayerRotation = playerObject.transform.rotation;
            originalCamera = playerCamera;
            
            // Disable player scripts
            SetPlayerScriptsEnabled(false);
            
            // Move player to sitting position
            float elapsed = 0f;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * transitionSpeed;
                playerObject.transform.position = Vector3.Lerp(originalPlayerPosition, sittingPosition.position, elapsed);
                playerObject.transform.rotation = Quaternion.Lerp(originalPlayerRotation, sittingPosition.rotation, elapsed);
                yield return null;
            }
            
            // Switch to monitor camera
            if (monitorCamera != null) {
                playerCamera.gameObject.SetActive(false);
                monitorCamera.gameObject.SetActive(true);
                playerCamera = monitorCamera;
            }
            
            // Hide sitting UI
            if (sittingUI != null) {
                sittingUI.SetActive(false);
            }
            
            isSitting = true;
            isTransitioning = false;
            
            Debug.Log("Player is now sitting at computer");
        }
        
        System.Collections.IEnumerator TransitionToStanding() {
            isTransitioning = true;
            
            // Switch back to player camera
            if (originalCamera != null) {
                playerCamera.gameObject.SetActive(false);
                originalCamera.gameObject.SetActive(true);
                playerCamera = originalCamera;
            }
            
            // Move player back to original position
            float elapsed = 0f;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * transitionSpeed;
                playerObject.transform.position = Vector3.Lerp(sittingPosition.position, originalPlayerPosition, elapsed);
                playerObject.transform.rotation = Quaternion.Lerp(sittingPosition.rotation, originalPlayerRotation, elapsed);
                yield return null;
            }
            
            // Re-enable player scripts
            SetPlayerScriptsEnabled(true);
            
            isSitting = false;
            isTransitioning = false;
            
            Debug.Log("Player has stood up from computer");
        }
        
        void SetPlayerScriptsEnabled(bool enabled) {
            foreach (MonoBehaviour script in playerScriptsToDisable) {
                if (script != null) {
                    script.enabled = enabled;
                }
            }
        }
        
        void OnDrawGizmosSelected() {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Draw sitting position
            if (sittingPosition != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(sittingPosition.position, 0.5f);
            }
        }
        
        public bool IsPlayerSitting() {
            return isSitting;
        }
        
        public bool IsPlayerInRange() {
            return playerInRange;
        }
    }
}
