namespace GameFish;

partial class BaseEntity
{
	/// <summary>
	/// Does this entity have a valid <see cref="global::GameFish.HealthComponent"/>?
	/// </summary>
	public bool HasHealth => HealthComponent.IsValid();

	[Title( "Component" )]
	[Property, Feature( IHealth.FEATURE )]
	public HealthComponent HealthComponent
	{
		get => _hp.IsValid() ? _hp : Components?.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
		set { _hp = value; }
	}
	private HealthComponent _hp;

	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsAlive
	{
		get => HealthComponent?.IsAlive ?? false;
		set { if ( HealthComponent.IsValid() ) HealthComponent.IsAlive = value; }
	}

	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsDestructible
	{
		get => HealthComponent?.IsDestructible ?? false;
		set { if ( HealthComponent.IsValid() ) HealthComponent.IsDestructible = value; }
	}

	[Title( "Initial" )]
	[Group( IHealth.GROUP_VALUES )]
	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public float Health
	{
		get => HealthComponent?.Health ?? 0f;
		set { if ( HealthComponent.IsValid() ) HealthComponent.Health = value; }
	}

	[Title( "Max" )]
	[Group( IHealth.GROUP_VALUES )]
	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public float MaxHealth
	{
		get => HealthComponent?.MaxHealth ?? 0f;
		set { if ( HealthComponent.IsValid() ) HealthComponent.MaxHealth = value; }
	}
}
