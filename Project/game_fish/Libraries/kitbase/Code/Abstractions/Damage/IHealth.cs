using System;

namespace GameFish;

public interface IHealth
{
    public const string FEATURE = "💖 Health";
	public const string GROUP_VALUES = "Values";

    abstract bool IsAlive { get; protected set; }

    /// <summary> Is this capable of ever taking damage? </summary>
    public bool IsDestructible { get; protected set; }

    public float Health { get; protected set; }
    public float MaxHealth { get; protected set; }

    public void SetHealth( in float hp )
    {
        Health = hp.Clamp( 0f, MaxHealth );

        if ( Health > 0 )
            Revive();
        else if ( Health <= 0 )
            Die();
    }

    public void ModifyHealth( in float hp )
        => SetHealth( Health + hp );

    public void Die()
    {
        if ( !IsAlive )
            return;

        if ( Health > 0f )
            Health = MathF.Min( 0f, Health );

        IsAlive = false;
        OnDeath();
    }

    public void Revive( bool restoreHealth = false )
    {
        if ( IsAlive )
            return;

        IsAlive = true;

        if ( restoreHealth )
            Health = MathF.Max( Health, MaxHealth );

        OnRevival();
    }

    protected void OnDeath() { }
    protected void OnRevival() { }

    public bool CanDamage( in DamageInfo dmgInfo )
        => IsDestructible && dmgInfo.Damage > 0;

    public bool TryDamage( in DamageInfo dmgInfo )
    {
        if ( !CanDamage( in dmgInfo ) )
            return false;

        ApplyDamage( dmgInfo );
        return true;
    }

    public void ApplyDamage( DamageInfo dmgInfo )
        => ModifyHealth( dmgInfo.Damage );
}
