using System;

namespace OmniCore.Services.Entities;

public class ProfileEntry
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
}