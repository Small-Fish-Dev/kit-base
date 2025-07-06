using System;
using System.Threading.Tasks;

namespace GameFish;

/// <summary>
/// A trigger volume with callbacks and no filters. <br />
/// Capable of creating, updating and rendering its collision.
/// </summary>
[Title( "Trigger" )]
[Icon( "highlight_alt" )]
[Group( COMPONENT_GROUP )]
[EditorHandle( "materials/tools/mesh_icons/quad.png" )]
public partial class BaseTrigger : Component, Component.ITriggerListener, Component.ExecuteInEditor
{
	public const string COMPONENT_GROUP = "Triggers";

	public const string GROUP_DEBUG = "🐞 Debug";
	public const string GROUP_COLLIDER = "🏀 Collider";
	public const string GROUP_CALLBACK = "⚡ Callbacks";

	public const int ORDER_CALLBACK = 420;

	public const string TAG_TRIGGER = "trigger";
	public const string TAG_LADDER = "ladder";

	public enum ColliderType
	{
		/// <summary>
		/// Doesn't create any colliders. Lets you add your own.
		/// </summary>
		Manual,

		/// <summary>
		/// Automatically creates and resizes a box collider.
		/// </summary>
		Box,

		/// <summary>
		/// Automatically creates a sphere collider.
		/// </summary>
		Sphere,
	}

	/// <summary>
	/// Allows automatically creating, updating and previewing a collider.
	/// </summary>
	[Property, Group( GROUP_COLLIDER )]
	public virtual ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}
	private ColliderType _colType = ColliderType.Box;

	public virtual bool UsingBox => Collider is ColliderType.Box;
	public virtual bool UsingSphere => Collider is ColliderType.Sphere;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public virtual BBox BoxSize
	{
		get => _boxSize;
		set { _boxSize = value; UpdateColliders(); }
	}
	private BBox _boxSize = new( new Vector3( -64, -128f, -128f ), new Vector3( 64f, 128f, 128f ) );

	[ShowIf( nameof( UsingSphere ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public float SphereRadius
	{
		get => _sphereRadius;
		set
		{
			_sphereRadius = value;
			UpdateColliders();
		}
	}
	private float _sphereRadius = 64f;

	/// <summary>
	/// Print debug logs?
	/// </summary>
	[Property, Group( GROUP_DEBUG )]
	public bool DebugLogging { get; set; } = false;

	/// <summary>
	/// Render gizmos in play mode?
	/// </summary>
	[Property, Group( GROUP_DEBUG )]
	public bool DebugGizmos { get; set; } = false;

	/// <summary> An object that passed filters just touched this. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnEnter { get; set; }

	/// <summary> An object that passed filters just exited this. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnExit { get; set; }

	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnFailedFilter { get; set; }

	/// <summary> A passing object just entered this it was previously empty. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnFirstEnter { get; set; }

	/// <summary> The last/only object occupying this trigger just exited. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnEmpty { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnInsideUpdate { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnInsideFixedUpdate { get; set; }

	/// <summary>
	/// Has <see cref="OnStart"/> been called yet?
	/// </summary>
	public bool Initialized { get; set; }

	/// <summary>
	/// Has this ever once been triggered before?
	/// </summary>
	public bool HasTriggered { get; set; }

	public List<GameObject> Touching { get; set; }

	public BoxCollider Box { get; set; }
	public SphereCollider Sphere { get; set; }

	public virtual TagSet DefaultTags { get; } = [TAG_TRIGGER];
	public virtual Color GizmoColor { get; } = Color.Green.Desaturate( 0.8f ).Darken( 0.2f );

	protected override Task OnLoad()
	{
		if ( !Scene.IsValid() )
			return base.OnLoad();

		// Update tags immediately.
		Tags?.Add( DefaultTags ?? [] );

		// Give us a box collider if we have none.
		if ( !Components.Get<Collider>( FindMode.EverythingInSelf ).IsValid() )
			Collider = ColliderType.Box;

		UpdateColliders();

		return base.OnLoad();
	}

	protected override void OnStart()
	{
		base.OnStart();

		UpdateColliders();

		Transform.OnTransformChanged += UpdateColliders;

		Initialized = true;
	}

	protected void DebugLog( params object[] log )
	{
		if ( DebugLogging )
			this.Log( log );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugGizmos )
			DrawGizmos();

		if ( this.InEditor() )
			return;

		if ( OnInsideUpdate is null || Touching is null )
			return;

		try
		{
			foreach ( var obj in Touching )
				OnInsideUpdate.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnInsideUpdate )} callback exception: {e}" );
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( this.InEditor() )
			return;

		if ( OnInsideFixedUpdate is null || Touching is null )
			return;

		try
		{
			foreach ( var obj in Touching )
				OnInsideFixedUpdate.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnInsideFixedUpdate )} callback exception: {e}" );
		}
	}

	protected virtual void UpdateColliders()
	{
		if ( !Scene.IsValid() )
			return;

		// Box
		if ( UsingBox )
		{
			if ( !Box.IsValid() )
				Box = Components.GetOrCreate<BoxCollider>( FindMode.EverythingInSelf );

			Box.Enabled = !(Scene?.IsEditor ?? true);
			Box.IsTrigger = true;

			Box.Scale = BoxSize.Size - BoxSize.Center;
			Box.Center = default;
		}
		else if ( Box.IsValid() )
		{
			Box.Enabled = false;
		}

		// Sphere
		if ( UsingSphere )
		{
			if ( !Sphere.IsValid() )
				Sphere = Components.GetOrCreate<SphereCollider>( FindMode.EverythingInSelf );

			Sphere.Enabled = !(Scene?.IsEditor ?? true);
			Sphere.IsTrigger = true;

			Sphere.Radius = SphereRadius;
		}
		else if ( Sphere.IsValid() )
		{
			Sphere.Enabled = false;
		}
	}

	public void OnTriggerEnter( GameObject obj )
	{
		if ( !PassesFilters( obj ) )
		{
			DebugLog( obj + " FAILED the filter " );

			if ( OnFailedFilter is not null )
			{
				try
				{
					OnFailedFilter.Invoke( this, obj );
				}
				catch ( Exception e )
				{
					this.Warn( $"OnFailedFilter callback exception: {e}" );
				}
			}

			return;
		}

		DebugLog( obj + " PASSED the filter" );

		OnTouchStart( obj );
	}

	public void OnTriggerExit( GameObject obj )
	{
		if ( obj is not null && (Touching?.Contains( obj ) ?? false) )
			OnTouchStop( obj );
	}

	protected virtual void OnTouchStart( GameObject obj )
	{
		Touching ??= [];

		var firstTouch = !Touching.Any( obj => obj.IsValid() );

		if ( !Touching.Contains( obj ) )
			Touching.Add( obj );

		try
		{
			if ( firstTouch )
				OnFirstEntered( obj );

			OnEnter?.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"OnEnter callback exception: {e}" );
		}

		// Let 'em know.
		HasTriggered = true;
	}

	protected virtual void OnTouchStop( GameObject obj )
	{
		Touching?.Remove( obj );

		// Validate
		Touching?.RemoveAll( obj => !PassesFilters( obj ) );

		// Callback
		OnExit?.Invoke( this, obj );
	}

	protected virtual void OnFirstEntered( GameObject obj )
	{
		OnFirstEnter?.Invoke( this, obj );
	}

	protected virtual void OnEmptied( GameObject obj )
	{
		OnEmpty?.Invoke( this, obj );
	}

	/// <returns> If the object is valid. </returns>
	protected virtual bool PassesFilters( GameObject obj )
	{
		return obj.IsValid();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		_ = Collider switch
		{
			ColliderType.Box => this.DrawBox( BoxSize.Translate( Box?.Center ?? default ), GizmoColor ),
			ColliderType.Sphere => this.DrawSphere( SphereRadius, Sphere?.Center ?? default, GizmoColor ),
			_ => false
		};
	}
}
