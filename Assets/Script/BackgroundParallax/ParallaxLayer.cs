using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    [SerializeField] private Transform background;
    [SerializeField] private float parallaxMutiplier;

    public void Move(float distanceToMove)
    {
        background.position += Vector3.right * (distanceToMove * parallaxMutiplier);
    }
}
