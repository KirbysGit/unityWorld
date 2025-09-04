using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool _isOpen = false;
    public bool isOpen 
    { 
        get { return _isOpen; }
        private set { _isOpen = value; }
    }
    
    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float speed = 1f;
    [Header("Rotation Configs")] 
    [SerializeField] private float rotationAmount = 90f;
    [SerializeField] private float ForwardDirection = 0;

    private Vector3 StartRotation;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;

    private void Awake(){
        StartRotation = transform.rotation.eulerAngles;
        Forward = transform.right;
        //Debug.Log($"Door {gameObject.name} initialized. Start rotation: {StartRotation}, Forward: {Forward}");
    }

    public void Open(Vector3 UserPosition){
        //Debug.Log($"Door.Open() called from position {UserPosition}");
        if (!isOpen){
            if (AnimationCoroutine != null){
                StopCoroutine(AnimationCoroutine);
            }
            if (isRotatingDoor){
                float dot = Vector3.Dot(Forward, (UserPosition - transform.position).normalized);
                //Debug.Log($"Dot Product: {dot.ToString("N3")}");
                AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
        }
        else {
            Debug.Log("Door is already open!");
        }
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Debug.Log("Starting door open animation");
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
        //Debug.Log("Door open animation complete");
    }

    public void Close(){
        //Debug.Log("Door.Close() called");
        if(isOpen){
            if(AnimationCoroutine != null){
                StopCoroutine(AnimationCoroutine);
            }
            if(isRotatingDoor){
                AnimationCoroutine = StartCoroutine(DoRotationClose());
            }
        }
        else {
            //Debug.Log("Door is already closed!");
        }
    }

    private IEnumerator DoRotationClose(){
        //Debug.Log("Starting door close animation");
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(StartRotation);

        isOpen = false;

        float time = 0;
        while (time < 1){
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
            
        }
        //Debug.Log("Door close animation complete");
    }
}