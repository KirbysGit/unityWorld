// Imports.
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private bool _isOpen = false;
    
    public bool isOpen 
    { 
        get { return _isOpen; }
        private set { _isOpen = value; }
    }
    
    private bool isRotatingDoor = true;
    private float speed = 1f;
    private float rotationAmount = 90f;
    private float ForwardDirection = 0;

    private Vector3 StartRotation;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;

    // -------------------------------------------------------- before first frame.
    private void Awake(){
        // gets initial rotation & forward direction.
        StartRotation = transform.rotation.eulerAngles;
        Forward = transform.right;
    }

    // -------------------------------------------------------- IInteractable implementation.
    public void Interact(Vector3 playerPosition)
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open(playerPosition);
        }
    }

    // -------------------------------------------------------- open the door.
    public void Open(Vector3 UserPosition){
        if (!isOpen){
            StopCoroutine(AnimationCoroutine);
            float dot = Vector3.Dot(Forward, (UserPosition - transform.position).normalized);
            AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
        }
    }

    // -------------------------------------------------------- open the door.
    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation;

        if (ForwardAmount >= ForwardDirection){
            endRotation = Quaternion.Euler(new Vector3(0, StartRotation.y - rotationAmount, 0));
        }
        else {
            endRotation = Quaternion.Euler(new Vector3(0, StartRotation.y + rotationAmount, 0));
        }

        isOpen = true;

        float time = 0;
        while (time < 1){
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    public void Close(){
        if(isOpen){
            StopCoroutine(AnimationCoroutine);

            AnimationCoroutine = StartCoroutine(DoRotationClose());
        }
    }

    private IEnumerator DoRotationClose(){
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(StartRotation);

        isOpen = false;

        float time = 0;
        while (time < 1){
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    public string GetPromptText()
    {
        return isOpen ? "Close \"E\"" : "Open \"E\"";
    }
}