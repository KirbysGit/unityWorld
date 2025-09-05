using TMPro;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] private GameObject UseTest;
    [SerializeField] private Transform Camera;
    [SerializeField] private float MaxUseDistance = 5f;
    [SerializeField] private LayerMask UseLayers;

    public void OnUse(){
        if (Physics.Raycast(Camera.position, Camera.forward, out RaycastHit hit, MaxUseDistance, UseLayers)){
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            if (hit.collider.TryGetComponent<Door>(out Door door)){
                Debug.Log("Door component found!");
                if (door.isOpen){
                    door.Close();
                }
                else {
                    door.Open(transform.position);
                }
            }
            else {
                Debug.Log("No Door component found on hit object");
            }
        }
        else {
            Debug.Log("No raycast hit within range");
        }
    }

    private void Update(){
        if (Physics.Raycast(Camera.position, Camera.forward, out RaycastHit hit, MaxUseDistance, UseLayers)){
            Debug.Log($"Update raycast hit: {hit.collider.name}");
            
            if (hit.collider.TryGetComponent<Door>(out Door door)){
                Debug.Log("Door found in Update - showing UI");
                
                // Get the TextMeshPro component from the GameObject
                TextMeshPro textMesh = UseTest.GetComponent<TextMeshPro>();
                if (textMesh != null){
                    if (door.isOpen){
                        textMesh.SetText("Close \"E\"");
                        Debug.Log("Set text to: Close \"E\"");
                    }
                    else {
                        textMesh.SetText("Open \"E\"");
                        Debug.Log("Set text to: Open \"E\"");
                    }
                }
                
                UseTest.SetActive(true);
                Debug.Log($"UI Text active: {UseTest.activeInHierarchy}");
            }
            else {
                Debug.Log("No Door component found in Update");
                UseTest.SetActive(false);
            }
        }
        else {
            UseTest.SetActive(false);
        }
    }
}