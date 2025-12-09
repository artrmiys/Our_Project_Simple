using UnityEngine;

/// <summary>
/// Lever is an interactable object that switches ON/OFF.
/// Implements IHighlightable for visual feedback.
/// Demonstrates polymorphism through override of Interact().
/// </summary>
public class Lever : Interactable, IHighlightable
{
    [Header("Lever State")]
    public bool isOn = false;

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
    /// Lever toggles ON/OFF when interacted with.
    /// </summary>
    public override void Interact()
    {
        isOn = !isOn;
        Debug.Log("Lever is now: " + (isOn ? "ON" : "OFF"));
    }

    /// <summary>
    /// Visual highlight when player is aiming at the lever.
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