using System.ComponentModel.DataAnnotations;

namespace KVS.Models;

/// <summary>
/// Our EFCore model for storing key value pairs in our persistance.<br/>
/// The <see cref="Key"/> of the <see cref="KeyValueModel"> is used as a primary key as we want keys to be unique.<br/>
/// </summary>
public sealed class KeyValueModel
{
    [Key]
    public required string Key { get; init; }
    public required string Value { get; set; }
}
