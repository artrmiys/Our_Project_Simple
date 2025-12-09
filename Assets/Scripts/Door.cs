using UnityEngine;

/// <summary>
/// Door is an interactable object that can open or close,
/// but only if a linked lever is switched ON.
/// Implements IHighlightable for visual feedback.
/// Demonstrates polymorphism through overriding Interact().
/// </summary>
public class Door : Interactable, IHighlightable
{
    [Header("Door State")]
    public bool isOpen = false;

    [Header("Linked Lever (Door only works if this lever is ON)")]
    public Lever linkedLever;

    [Header("Highlight Materials")]
    public Material highlightMaterial;

    private Material originalMaterial;
    private Renderer rend;

    void Start()
    {
        // Get renderer from this object or child meshes
        rend = GetComponentInChildren<Renderer>();

        if (rend != null)
            originalMaterial = rend.material;
    }

    /// <summary>
    /// Polymorphic behavior:
    /// Door reacts to interaction ONLY if lever is ON.
    /// </summary>
    public override void Interact()
    {
        // Prevent interaction if lever is assigned and turned off
        if (linkedLever != null && !linkedLever.isOn)
        {
            Debug.Log("Door is locked. Lever is OFF.");
            return;
        }

        // Toggle door state
        isOpen = !isOpen;
        Debug.Log("Door is now: " + (isOpen ? "Open" : "Closed"));
    }

    /// <summary>
    /// Visual highlight when player is aiming at the door.
    /// </summary>
    public void Highlight()
    {
        if (rend != null && highlightMaterial != null)
            rend.material = highlightMaterial;
    }

    /// <summary>
    /// Remove highlight when player looks away.
    /// </summary>
    public void Unhighlight()
    {
        if (rend != null && originalMaterial != null)
            rend.material = originalMaterial;
    }
}