namespace KVS.Messages;

/// <summary>
/// A <see cref="KeyModified"/> message that is sent when a key value pair is deleted.
/// </summary>
/// <param name="NodeId">The node that sent the <see cref="KeyModified"/> message.</param>
/// <param name="ModifiedKey">The key to be updated on other nodes' caches.</param>
public sealed record KeyModified(Guid NodeId, string ModifiedKey);
