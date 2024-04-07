namespace KVS.Messages;

/// <summary>
/// A <see cref="KeyDeletion"/> message that is sent when a key value pair is deleted.
/// </summary>
/// <param name="NodeId">The node that sent the <see cref="KeyDeletion"/> message</param>
/// <param name="DeletedKey">The key to be deleted from other nodes' caches</param>
public sealed record KeyDeletion(Guid NodeId, string DeletedKey);
