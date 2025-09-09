using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    
    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        float cameraPositionX = mainCamera.transform.position.x;
        float distanceToMove = cameraPositionX - lastCameraPositionX;
        lastCameraPositionX = cameraPositionX;
        
        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
        }
    }
    
}
