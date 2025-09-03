using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Seagull.Interior_01 {
    public class ComputerInteraction : MonoBehaviour {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private Key interactKey = Key.E;
        
        [Header("Camera System")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Camera monitorCamera;
        [SerializeField] private MonitorCamLook monitorCamLook; // Reference to the monitor camera look script
        
        [Header("UI Elements")]
        [SerializeField] private GameObject interactionPrompt;
        
        [Header("Player Control")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Movement movementScript;
        [SerializeField] private GameObject playerModel;  // The visual model/body of the player

        
        private bool playerInRange = false;
        private bool isSitting = false;
        private bool isTransitioning = false;
        

        // -------------------------------------------------------- Before First Frame.
        void Start() {
            // Check If Prompt Is Assigned.
            monitorCamera.enabled = false;
            InitializeUI();

            if (playerObject != null) {
                movementScript = playerObject.GetComponent<Movement>();
            }
            Debug.Log("ComputerInteraction: Movement Script: " + movementScript);
        }

        void InitializeUI() {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(false);
            }
        }
        
        // -------------------------------------------------------- Every Frame.
        void Update() {
            // Check If Player Is In Range.
            CheckPlayerProximity();

            // Handle Input.
            HandleInput();
        }
        
        void CheckPlayerProximity() {
            // Set Distance Between Player And Computer.
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            // Bool For If Player Was In Range.
            bool wasInRange = playerInRange;

            // Set If Player Is In Range.
            playerInRange = distance <= interactionRange;

            // If Player Entered Range.
            if (playerInRange && !wasInRange) {
                // Show Prompt.
                ShowInteractionPrompt();

                // If Player Is Not Sitting.
                if (!isSitting) {
                    // Show Prompt.
                    ShowInteractionPrompt();
                }
            } else if (!playerInRange && wasInRange) {
                // Hide Prompt.
                if (!isSitting) {
                    HideInteractionPrompt();
                }
            }
        }
        
        // Handle Input.
        void HandleInput() {
            // If Keyboard Is Assigned.
            if (Keyboard.current != null) {
                // If Interact Key Was Pressed.
                if (Keyboard.current[interactKey].wasPressedThisFrame) {
                    
                    // If Player Is In Range And Not Sitting.
                    if (playerInRange && !isSitting) {
                        // Sit Down.
                        SitDown();
                    } else if (isSitting) {
                        // Stand Up.
                        StandUp();
                    }
                }
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
        
        public void SitDown() {
            if (isTransitioning) return;

            
            HideInteractionPrompt();
            StartCoroutine(TransitionToSitting());
        }
        
        public void StandUp() {
            if (isTransitioning) return;
            
            StartCoroutine(TransitionToStanding());
        }
        
        System.Collections.IEnumerator TransitionToSitting() {
            isTransitioning = true;
            
            playerCamera.enabled = false;
            monitorCamera.enabled = true;
            movementScript.enabled = false;
            playerModel.SetActive(false);
            monitorCamLook.SetAllowLook(true);
            
            isSitting = true;
            isTransitioning = false;
            
            yield return null;
        }
        
        System.Collections.IEnumerator TransitionToStanding() {
            isTransitioning = true;
        
            monitorCamera.enabled = false;
            
            // Enable Player Camera
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
            
            movementScript.enabled = true;
            playerModel.SetActive(true);
            monitorCamLook.SetAllowLook(false);

            isSitting = false;
            isTransitioning = false;
            
            // Wait A Frame And Check Camera States Again
            yield return null;
        }
        
        public bool IsPlayerSitting() {
            return isSitting;
        }
        
        public bool IsPlayerInRange() {
            return playerInRange;
        }
        
        // Method to set sitting state (for MonitorFocus coordination)
        public void SetSittingState(bool sitting) {
            isSitting = sitting;
        }
    }
}
