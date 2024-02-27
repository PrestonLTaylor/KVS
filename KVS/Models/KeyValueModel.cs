using System.ComponentModel.DataAnnotations;

namespace KVS.Models;

public sealed class KeyValueModel
{
    [Key]
    public required string Key { get; init; }
    public required string Value { get; set; }
}
