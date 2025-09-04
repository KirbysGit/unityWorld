using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    private void Update()
    {
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
        else
        {
            Debug.LogWarning("cameraPosition not assigned in MoveCamera script. Please assign it in the inspector.");
        }
    }
}
