using UnityEngine;

/// <summary>
/// Abstract base class representing a generic stage action.
/// Any subclass must implement the Execute() method.
/// </summary>
public abstract class ActionBase : MonoBehaviour
{
    /// <summary>
    /// Abstract method to be implemented by subclasses.
    /// Defines the action's behavior.
    /// </summary>
    public abstract void Execute();
}
