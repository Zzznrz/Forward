using UnityEngine;

/// <summary>Invisible checkpoint used for nearby respawns after a fall.</summary>
public class CheckpointZone : MonoBehaviour
{
    // Optional explicit respawn transform; the checkpoint itself is the fallback.
    [SerializeField] private Transform respawnPoint;

    public Vector3 RespawnPosition => respawnPoint != null
        ? respawnPoint.position
        : transform.position;
}
