using System;
using System.Collections.Generic;

namespace GmlCore.Interfaces.Mods;

/// <summary>Saving these dependencies would make an optional mod need itself.</summary>
public class ModDependencyCycleException(IReadOnlyList<string> cycle)
    : InvalidOperationException($"Циклическая зависимость: {string.Join(" → ", cycle)}")
{
    public IReadOnlyList<string> Cycle { get; } = cycle;
}
