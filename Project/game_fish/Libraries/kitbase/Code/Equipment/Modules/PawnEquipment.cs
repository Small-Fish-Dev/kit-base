using System;

namespace GameFish;

/// <summary>
/// Manages and optionally spawns equipment.
/// </summary>
public partial class PawnEquipment : Module<BasePawn>
{
	public const string FEATURE_EQUIPS = "🏹 Equipment";

	public const string GROUP_SLOTTING = "Slotting";
	public const string GROUP_INVENTORY = "Inventory";

	/// <summary> If true: only try to pick up weapons in their intended slot. </summary>
	[Group( GROUP_SLOTTING ), Title( "Strict" )]
	[Property, Feature( FEATURE_EQUIPS )]
	public virtual bool StrictSlots { get; set; }

	/// <summary> If true: prevent picking up multiple instances of a weapon. </summary>
	[Group( GROUP_SLOTTING ), Title( "Unique" )]
	[Property, Feature( FEATURE_EQUIPS )]
	public virtual bool UniqueEquips { get; set; } = true;

	/// <summary> How many weapons can fit in each individual slot? </summary>
	[Group( GROUP_SLOTTING ), Title( "Size" )]
	[Range( 1, 4, step: 1, clamped: false )]
	[Property, Feature( FEATURE_EQUIPS )]
	public virtual int SlotCapacity { get; set; } = 1;

	/// <summary> How many equipment slots are available overall? </summary>
	[Group( GROUP_SLOTTING ), Title( "Available" )]
	[Range( 0, 10, step: 1, clamped: false )]
	[Property, Feature( FEATURE_EQUIPS )]
	public virtual int SlotCount { get; set; } = 10;

	/// <summary>
	/// Automatically give the loadout when this module first starts? <br />
	/// If not then you'll need to call <see cref="GiveLoadout"/> yourself.
	/// </summary>
	[Property, Feature( FEATURE_EQUIPS ), Group( GROUP_INVENTORY )]
	public virtual bool AutoGiveLoadout { get; set; } = true;

	/// <summary> The weapons to spawn. </summary>
	[Property, Feature( FEATURE_EQUIPS ), Group( GROUP_INVENTORY )]
	public virtual List<EquipLoadoutEntry> Loadout { get; set; } = [];

	[InlineEditor, ReadOnly]
	[Sync( SyncFlags.FromHost )]
	[HideIf( nameof( EditingScene ), true )]
	[Property, Feature( FEATURE_EQUIPS ), Group( GROUP_INVENTORY )]
	public NetList<BaseEquip> Equipped { get; } = [];

	/// <summary>
	/// The equipment actively deployed(if any).
	/// </summary>
	public BaseEquip ActiveEquip
	{
		get => _activeEquip;
		set
		{
			if ( !value.IsValid() )
			{
				Holster( _activeEquip );
				_activeEquip = value;

				return;
			}

			if ( _activeEquip == value )
				return;

			Holster( _activeEquip );

			_activeEquip = value;
			Deploy( value );
		}
	}

	protected BaseEquip _activeEquip;

	public virtual void GiveLoadout()
	{
		if ( Loadout is null )
			return;

		foreach ( var entry in Loadout )
		{
			this.Log( entry );
			TryEquip( entry.Prefab, entry.Slot?.AsInt() );
		}
	}

	public virtual BaseEquip SelectFirstEquip()
	{
		if ( Equipped is null )
			return null;

		return Equipped.OrderBy( e => e?.Slot ?? 0 )
			.FirstOrDefault( e => e.IsValid() );
	}

	protected virtual void Holster( BaseEquip e )
	{
		if ( !e.IsValid() )
			return;

		e.EquipState = EquipState.Holstered;
		e.OnHolster( this );
	}

	protected virtual void Deploy( BaseEquip e )
	{
		if ( !e.IsValid() )
			return;

		this.Log( $"Deployed equip:[{e}]" );

		e.EquipState = EquipState.Deployed;
		e.OnDeploy( this );
	}

	public virtual bool TryRemove( BaseEquip e )
	{
		if ( !e.IsValid() )
		{
			// Check if it was destroyed but still referenced.
			if ( e is not null && Equipped is not null )
				if ( Equipped.Contains( e ) )
					RefreshList();

			return false;
		}

		e.GameObject.Destroy();

		RefreshList();

		return true;
	}

	public virtual bool TryDrop( BaseEquip e )
	{
		throw new NotImplementedException();
	}

	public virtual bool TryEquip( PrefabFile p, int? slot = null )
	{
		if ( !p.TrySpawn( WorldPosition, out var go ) )
		{
			this.Warn( $"Tried to equip invalid equipment prefab:[{p}]" );
			return false;
		}

		if ( TryEquip( go.Components.Get<BaseEquip>( FindMode.EverythingInSelf ) ) )
			return true;

		go.Destroy();
		return false;
	}

	public virtual bool TryEquip( BaseEquip e, int? slot = null )
	{
		if ( !ParentComponent.IsValid() )
		{
			this.Warn( $"Tried to equip:[{e}] with an invalid parent:[{ParentComponent}]" );
			return false;
		}

		if ( !e.IsValid() )
		{
			this.Warn( $"Tried to equip an invalid on parent:[{ParentComponent}]" );
			return false;
		}

		// Allow restricting one weapon at a time.
		if ( UniqueEquips && Any( e.ID ) )
			return false;

		// Allow restricting to a specific slot.
		if ( StrictSlots )
		{
			slot ??= e.DefaultSlot;

			if ( slot != e.DefaultSlot )
				return false;
		}
		else
		{
			slot ??= FirstFreeSlot();

			if ( !slot.HasValue )
				return false;
		}

		var inSlot = GetInSlot( slot );

		if ( inSlot.Count() > SlotCapacity )
		{
			this.Log( $"Tried to equip:[{e}] in full slot:[{slot}] on parent:[{ParentComponent}]" );
			return false;
		}

		// Equip might have something to say about this.
		if ( !e.AllowEquip( ParentComponent ) )
			return false;

		e.EquipState = ActiveEquip == e
			? EquipState.Deployed
			: EquipState.Holstered;

		e.GameObject.SetParent( GameObject, keepWorldPosition: false );
		e.LocalPosition = Vector3.Zero;

		Equipped?.Add( e );

		e.Owner = ParentComponent;
		e.Inventory = this;

		e.OnEquip( this );

		if ( !ActiveEquip.IsValid() )
			ActiveEquip = e;

		return true;
	}
}
