namespace GameFish;

partial class BaseEntity
{
    [Sync]
    [Property, Feature( IHealth.FEATURE )]
    public bool IsAlive { get; set; }

    [Property, Feature( IHealth.FEATURE )]
    public virtual bool IsDestructible { get; set; }

    [Sync]
    [Property, Feature( IHealth.FEATURE )]
    public float Health { get; set; } = 100f;

    [Sync]
    [Property, Feature( IHealth.FEATURE )]
    public float MaxHealth { get; set; } = 100f;
}
