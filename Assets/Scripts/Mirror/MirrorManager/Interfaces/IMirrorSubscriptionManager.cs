
// IMirrorSubscriptionManager.cs
public interface IMirrorSubscriptionManager
{
    void SubscribeAll();
    void UnsubscribeAll();
    void SubscribeMirror(MirrorMoveController mirror);
    void UnsubscribeMirror(MirrorMoveController mirror);
}