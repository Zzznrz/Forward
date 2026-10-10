using UnityEngine;

/// <summary>End-of-level trigger with a replay prompt.</summary>
public class FinishZone : MonoBehaviour
{
    // Player reaches the finish marker and freezes in the completed state.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null)
            return;

        player.CompleteLevel();
        FindAnyObjectByType<LevelBuilder>()?.ShowCompletionScreen();
    }
}
