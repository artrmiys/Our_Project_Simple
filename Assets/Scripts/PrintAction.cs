using UnityEngine;

/// <summary>
/// Simple implementation of the abstract ActionBase class.
/// Executes by printing a message to the console.
/// </summary>
public class PrintAction : ActionBase
{
    public override void Execute()
    {
        Debug.Log("PrintAction executed.");
    }
}