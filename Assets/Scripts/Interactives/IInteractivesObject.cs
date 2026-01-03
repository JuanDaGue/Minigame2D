public interface IInteractivesObject
{
    /// <summary>
    /// Returns true if this object can currently be interacted with.
    /// </summary>
    bool CanInteract();

    /// <summary>
    /// Called when the player interacts with this object.
    /// </summary>
    void OnInteract();
}

