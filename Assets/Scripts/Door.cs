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
        if (isAnimating) return;
        
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    
    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
            
        animationCoroutine = StartCoroutine(AnimateDoor(closeAngle, openAngle, true));
        
        // Play open sound
        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);
    }
    
    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
            
        animationCoroutine = StartCoroutine(AnimateDoor(openAngle, closeAngle, false));
        
        // Play close sound
        if (audioSource && closeSound)
            audioSource.PlayOneShot(closeSound);
    }
    
    private IEnumerator AnimateDoor(float startAngle, float endAngle, bool opening)
    {
        isAnimating = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            float currentAngle = Mathf.Lerp(startAngle, endAngle, curveValue);
            SetDoorRotation(currentAngle);
            
            yield return null;
        }
        
        // Ensure final position is exact
        SetDoorRotation(endAngle);
        
        isOpen = opening;
        isAnimating = false;
        
        // Notify other systems
        OnDoorStateChanged?.Invoke(isOpen);
        
        Debug.Log($"Door {(opening ? "opened" : "closed")}");
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
