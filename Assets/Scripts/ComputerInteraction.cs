
// handling interaction with computer between walking around like normal, and entering the monitor view.

// Imports.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Seagull.Interior_01 {
    public class ComputerInteraction : MonoBehaviour {

        [Header("the cams ---------------------------------------------")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Camera threeDCamera;
        [SerializeField] private Camera monitorCamera;
        [SerializeField] private MonitorCamLook monitorCamLook;
        
        [Header("ui prompt --------------------------------------------")]
        [SerializeField] private GameObject interactionPrompt;
        
        [Header("the player -------------------------------------------")]
        [SerializeField] private GameObject playerModel;
        [SerializeField] private Movement movementScript;

        // State Variables.
        private bool isSitting = false;
        private Key interactKey = Key.E;
        private bool playerInRange = false;
        private float interactionRange = 3f;
        private bool isTransitioning = false;
        

        // -------------------------------------------------------- Before First Frame.
        void Start() {
            // ensure 3D camera is active at start
            threeDCamera.enabled = true;

            // standing at start.
            isSitting = false;
            
            // disable monitorCam.
            monitorCamera.enabled = false;

            // disable interactionPrompt.
            interactionPrompt.SetActive(false);
        }
        
        // -------------------------------------------------------- Every Frame.
        void Update() {
            // check if player close enough to computer.
            CheckPlayerProximity();

            // handle input.
            HandleInput();
        }

        // -------------------------------------------------------- check players range & show/hide prompt.
        
        void CheckPlayerProximity() {
            // get distance between player & computr, then set if in range.
            float distance = Vector3.Distance(transform.position, movementScript.transform.position); 
            bool wasInRange = playerInRange;                                                        
            playerInRange = distance <= interactionRange;                                           

            // if player in range & was not before, show the prompt.
            // else if, player not in range and was before, hide prompt.
            if (playerInRange && !wasInRange) {
                if (!isSitting) {
                    interactionPrompt.SetActive(true);
                }
            } else if (!playerInRange && wasInRange) {
                if (!isSitting) {
                    interactionPrompt.SetActive(false);
                }
            }
        }
        
        void HandleInput() {
            // if interact key pressed.
            if (Keyboard.current[interactKey].wasPressedThisFrame) {
                // if player in range and not sitting, sit down.
                // else if player is sitting, stand up.
                if (playerInRange && !isSitting) {
                    SitDown();
                } else if (isSitting) {
                    StandUp();
                }
            }
        }
        
        // -------------------------------------------------------- sit down & stand up funcs.
        public void SitDown() {
            if (isTransitioning) return;
            interactionPrompt.SetActive(false);
            StartCoroutine(TransitionToSitting());
        }
        
        public void StandUp() {
            if (isTransitioning) return;
            StartCoroutine(TransitionToStanding());
        }
        
        // -------------------------------------------------------- sit down coroutine.
        System.Collections.IEnumerator TransitionToSitting() {
            // update transitioning to true.
            isTransitioning = true;
            
            // disable playerCam & enable monitorCam.
            threeDCamera.enabled = false;
            playerCamera.enabled = false;
            monitorCamera.enabled = true;

            // disable movementScript & playerModel.
            movementScript.enabled = false;
            playerModel.SetActive(false);

            // allow user to look around.
            monitorCamLook.SetAllowLook(true);

            // update sitting state to true & transitioning to false.
            isSitting = true;
            isTransitioning = false;
            
            yield return null;
        }
        
        // -------------------------------------------------------- stand up coroutine.
        System.Collections.IEnumerator TransitionToStanding() {
            // update transitioning to true.
            isTransitioning = true;
        
            // disable monitorCam.
            threeDCamera.enabled = true;    
            monitorCamera.enabled = false;

            // enable movementScript.
            movementScript.enabled = true;
            
            // enable playerModel.
            playerModel.SetActive(true);
            
            // disable monitor camera look.
            monitorCamLook.SetAllowLook(false);

            // update sitting state to false & transitioning to false.
            isSitting = false;
            isTransitioning = false;
            
            yield return null;
        }
    }
}
