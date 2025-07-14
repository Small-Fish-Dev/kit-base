using System;
using System.Runtime.CompilerServices;

namespace GameFish;

/// <summary>
/// Takes damage and calls health-related events/hooks.
/// Add this to the root of the prefab/object you want to have health.
/// </summary>
[Icon( "monitor_heart" )]
public partial class HealthComponent : Component, IHealth
{
	[Sync]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsAlive { get; set; } = true;

    /// <summary> Is this capable of ever taking damage? </summary>
	[Property, Feature( IHealth.FEATURE )]
	public virtual bool IsDestructible { get; set; } = true;

	[Sync]
	[Property, Title( "Initial" )]
	[ShowIf( nameof( IsDestructible ), true )]
	[Group( IHealth.GROUP_VALUES ), Feature( IHealth.FEATURE )]
	public float Health { get; set; } = 100f;

	[Sync]
	[Property, Title( "Max" )]
	[ShowIf( nameof( IsDestructible ), true )]
	[Group( IHealth.GROUP_VALUES ), Feature( IHealth.FEATURE )]
	public float MaxHealth { get; set; } = 100f;
}
