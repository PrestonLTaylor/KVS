namespace KVS.Messages;

public sealed record KeyDeletion(Guid NodeId, string DeletedKey);
