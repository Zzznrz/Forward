using UnityEngine;

/// <summary>Keeps the camera centered on the player's horizontal progress.</summary>
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;

    private float cameraVelocityX;

    private void LateUpdate()
    {
        if (target == null)
            return;

        float nextX = Mathf.SmoothDamp(
            transform.position.x,
            target.position.x,
            ref cameraVelocityX,
            smoothTime);

        transform.position = new Vector3(
            nextX,
            transform.position.y,
            transform.position.z);
    }
}
