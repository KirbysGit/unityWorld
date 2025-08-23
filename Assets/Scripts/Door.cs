using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    [Header("References")]
    [SerializeField] private Transform doorPivot; // The pivot point for rotation
    
    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    // Events for other systems to listen to
    public System.Action<bool> OnDoorStateChanged;
    
    private void Start()
    {
        // If no door pivot is assigned, use the door itself
        if (doorPivot == null)
            doorPivot = transform;
            
        // Ensure door starts closed
        SetDoorRotation(closeAngle);
    }
    
    public void ToggleDoor()
    {
        Debug.Log($"[DOOR DEBUG] ToggleDoor called for {gameObject.name}. Current state: {(isOpen ? "OPEN" : "CLOSED")}, Animating: {isAnimating}");
        
        if (isAnimating) 
        {
            Debug.LogWarning($"[DOOR DEBUG] {gameObject.name}: Cannot toggle - door is currently animating!");
            return;
        }
        
        if (isOpen)
        {
            Debug.Log($"[DOOR DEBUG] {gameObject.name}: Closing door...");
            CloseDoor();
        }
        else
        {
            Debug.Log($"[DOOR DEBUG] {gameObject.name}: Opening door...");
            OpenDoor();
        }
    }
    
    public void OpenDoor()
    {
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: OpenDoor called. Current state: {(isOpen ? "OPEN" : "CLOSED")}, Animating: {isAnimating}");
        
        if (isOpen || isAnimating) 
        {
            Debug.LogWarning($"[DOOR DEBUG] {gameObject.name}: Cannot open - door is {(isOpen ? "already open" : "currently animating")}!");
            return;
        }
        
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: Starting open animation from {closeAngle}° to {openAngle}°");
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
            
        animationCoroutine = StartCoroutine(AnimateDoor(closeAngle, openAngle, true));
        
        // Play open sound
        if (audioSource && openSound)
        {
            audioSource.PlayOneShot(openSound);
            Debug.Log($"[DOOR DEBUG] {gameObject.name}: Playing open sound");
        }
        else
        {
            Debug.LogWarning($"[DOOR DEBUG] {gameObject.name}: No audio source or open sound assigned!");
        }
    }
    
    public void CloseDoor()
    {
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: CloseDoor called. Current state: {(isOpen ? "OPEN" : "CLOSED")}, Animating: {isAnimating}");
        
        if (!isOpen || isAnimating) 
        {
            Debug.LogWarning($"[DOOR DEBUG] {gameObject.name}: Cannot close - door is {(!isOpen ? "already closed" : "currently animating")}!");
            return;
        }
        
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: Starting close animation from {openAngle}° to {closeAngle}°");
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
            
        animationCoroutine = StartCoroutine(AnimateDoor(openAngle, closeAngle, false));
        
        // Play close sound
        if (audioSource && closeSound)
        {
            audioSource.PlayOneShot(closeSound);
            Debug.Log($"[DOOR DEBUG] {gameObject.name}: Playing close sound");
        }
        else
        {
            Debug.LogWarning($"[DOOR DEBUG] {gameObject.name}: No audio source or close sound assigned!");
        }
    }
    
    private IEnumerator AnimateDoor(float startAngle, float endAngle, bool opening)
    {
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: Animation started - {(opening ? "opening" : "closing")} from {startAngle}° to {endAngle}° over {animationDuration}s");
        
        isAnimating = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            float currentAngle = Mathf.Lerp(startAngle, endAngle, curveValue);
            SetDoorRotation(currentAngle);
            
            // Debug progress every 10 frames to avoid spam
            if (Time.frameCount % 10 == 0)
            {
                Debug.Log($"[DOOR DEBUG] {gameObject.name}: Animation progress: {elapsedTime:F2}s / {animationDuration}s ({normalizedTime * 100:F1}%) - Angle: {currentAngle:F1}°");
            }
            
            yield return null;
        }
        
        // Ensure final position is exact
        SetDoorRotation(endAngle);
        
        isOpen = opening;
        isAnimating = false;
        
        Debug.Log($"[DOOR DEBUG] {gameObject.name}: Animation completed! Door is now {(opening ? "OPEN" : "CLOSED")}");
        
        // Notify other systems
        OnDoorStateChanged?.Invoke(isOpen);
    }
    
    private void SetDoorRotation(float angle)
    {
        if (doorPivot != null)
        {
            Vector3 rotation = doorPivot.localEulerAngles;
            rotation.y = angle;
            doorPivot.localEulerAngles = rotation;
        }
    }
    
    // Public getters
    public bool IsOpen => isOpen;
    public bool IsAnimating => isAnimating;
    
    // Optional: Add visual feedback in the editor
    private void OnDrawGizmosSelected()
    {
        if (doorPivot != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(doorPivot.position, 0.1f);
            
            // Draw open and closed positions
            Gizmos.color = Color.green;
            Vector3 openPos = doorPivot.position + Quaternion.Euler(0, openAngle, 0) * Vector3.forward * 0.5f;
            Gizmos.DrawLine(doorPivot.position, openPos);
            
            Gizmos.color = Color.red;
            Vector3 closedPos = doorPivot.position + Quaternion.Euler(0, closeAngle, 0) * Vector3.forward * 0.5f;
            Gizmos.DrawLine(doorPivot.position, closedPos);
        }
    }
}
