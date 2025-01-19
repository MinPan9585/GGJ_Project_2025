using UnityEngine;

public class DeactivateAfter : MonoBehaviour
{
    // Time in seconds before the GameObject is deactivated
    public float deactivateAfterSeconds = 5f;

    private void OnEnable()
    {
        // Start the deactivation countdown
        Invoke("DeactivateGameObject", deactivateAfterSeconds);
    }

    private void DeactivateGameObject()
    {
        // Deactivate the GameObject
        gameObject.SetActive(false);
    }
}

