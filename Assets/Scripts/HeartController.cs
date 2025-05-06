using UnityEngine;

public class HeartController : MonoBehaviour
{
    /// <summary>
    /// Called when the player brings an orb in range and presses E.
    /// You can expand this to check an ordered queue or orb types.
    /// </summary>
    public void ResolveOrb(OrbController orb)
    {
        // Play pulse animation, SFX, update score, advance queue, etc.
        Debug.Log($"Orb delivered: {orb.name}");
    }
}