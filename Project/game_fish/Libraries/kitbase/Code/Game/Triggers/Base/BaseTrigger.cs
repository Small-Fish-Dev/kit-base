using System;
using System.Threading.Tasks;

namespace GameFish;

/// <summary>
/// A trigger volume with various filters. <br />
/// Capable of creating, updating and previewing its own collider.
/// </summary>
[Icon( "highlight_alt" )]
[Title( "Filtered Trigger" )]
[EditorHandle( "materials/tools/mesh_icons/quad.png" )]
public class BaseTrigger : Component, Component.ITriggerListener
{
	public const string GROUP_DEBUG = "🐞 Debug";
	public const string GROUP_COLLIDER = "🏀 Collider";
	public const string GROUP_FILTER_FUNC = "💻 Function Filter";
	public const string GROUP_FILTER_TAGS = "🏳 Tag Filter";
	public const string GROUP_FILTER_TYPE = "⌨ Type Filter";
	public const string GROUP_CALLBACK = "⚡ Callbacks";

	const int ORDER_FILTER_TAGS = 69;
	const int ORDER_FILTER_TYPE = 88;
	const int ORDER_CALLBACK = 420;

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
	public ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}
	private ColliderType _colType;

	public bool UsingBox => Collider is ColliderType.Box;
	public bool UsingSphere => Collider is ColliderType.Sphere;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public BBox BoxSize
	{
		get => _boxSize;
		set
		{
			_boxSize = value;
			UpdateColliders();
		}
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

	/// <summary>
	/// If true: include/exclude by type.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public bool FilterType { get; set; }

	/// <summary>
	/// They must have this type of component on them.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public Type RequireType { get; set; } = typeof( BasePlayer );

	/// <summary>
	/// How to look for the component.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public FindMode FindMode { get; set; } = FindMode.EnabledInSelf | FindMode.InAncestors | FindMode.InDescendants;

	/// <summary>
	/// If true: include/exclude by tags.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public bool FilterTags { get; set; } = true;

	/// <summary>
	/// An object with any of these tags are accepted.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[ShowIf( nameof( FilterTags ), true )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public TagFilter IncludeTags { get; set; } = new() { Enabled = true, Tags = [Entity.TAG_PLAYER] };

	/// <summary>
	/// An object with any of these tags are always ignored. <br />
	/// They're excluded even if they have an <see cref="IncludeTags"/> tag.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Property, Group( GROUP_FILTER_TAGS )]
	[ShowIf( nameof( FilterTags ), true )]
	public TagFilter ExcludeTags { get; set; }

	/// <summary>
	/// An object must have all of these these tags to trigger this.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Property, Group( GROUP_FILTER_TAGS )]
	[ShowIf( nameof( FilterTags ), true )]
	public TagFilter RequireTags { get; set; }

	/// <summary>
	/// An additional, final check you can do in ActionGraph.
	/// </summary>
	[Order( ORDER_FILTER_TYPE + 1 )]
	[Property, Group( GROUP_FILTER_FUNC )]
	public Func<BaseTrigger, GameObject, bool> PassesFilter { get; set; }

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

	public virtual Color GizmoColor { get; } = Color.Green.Desaturate( 0.5f ).Darken( 0.1f );

	protected override Task OnLoad()
	{
		if ( !Scene.IsValid() )
			return base.OnLoad();

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

		Initialized = true;

		Transform.OnTransformChanged += UpdateColliders;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugGizmos )
			DrawGizmos();

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

		if ( OnInsideFixedUpdate is null || Touching is null )
			return;

		try
		{
			foreach ( var obj in Touching )
				OnInsideUpdate.Invoke( this, obj );
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

		Tags?.Add( "trigger" );

		// Box
		if ( UsingBox )
		{
			if ( !Box.IsValid() )
				Box = Components.GetOrCreate<BoxCollider>( FindMode.EverythingInSelf );

			Box.Enabled = !(Scene?.IsEditor ?? true);
			Box.IsTrigger = true;

			Box.Scale = BoxSize.Size;
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

	protected void DebugLog( params object[] log )
	{
		if ( DebugLogging )
			this.Log( log );
	}

	/// <returns> If the object is valid and passes this trigger's tag filters(if any) and custom filter(if any). </returns>
	protected virtual bool TryPassFilter( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		if ( !TagsPassFilters( obj.Tags ) )
			return false;

		if ( !TypesPassFilters( obj ) )
			return false;

		if ( PassesFilter is not null )
		{
			try
			{
				return PassesFilter.Invoke( this, obj );
			}
			catch ( Exception e )
			{
				this.Warn( $"PassesFilter callback exception: {e}" );
			}
		}

		return true;
	}

	/// <summary>
	/// Returns if this set of tags is allowed. <br />
	/// Defaults to true if <see cref="FilterTags"/> is disabled.
	/// </summary>
	public bool TypesPassFilters( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		if ( !FilterType || RequireType is null )
			return true;

		return obj.Components.Get( RequireType, FindMode ).IsValid();
	}

	/// <summary>
	/// Returns if this set of tags is allowed. <br />
	/// Defaults to true if <see cref="FilterTags"/> is disabled.
	/// </summary>
	public bool TagsPassFilters( ITagSet tags )
	{
		if ( !FilterTags )
			return true;

		if ( tags is null )
			return false;

		var passed = false;

		// Include
		if ( IncludeTags.HasAny( tags ) )
			passed = true;

		// Exclude
		if ( ExcludeTags.HasAny( tags ) )
			return false;

		// Require
		if ( !tags.HasAll( RequireTags.Tags ?? [] ) )
			return false;

		return passed;
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
		Touching?.RemoveAll( obj => !TryPassFilter( obj ) );

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

	public void OnTriggerEnter( GameObject obj )
	{
		if ( !TryPassFilter( obj ) )
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
		if ( Touching?.Contains( obj ) ?? false )
			OnTouchStop( obj );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		_ = Collider switch
		{
			ColliderType.Box => this.DrawBox( BoxSize, GizmoColor ),
			ColliderType.Sphere => this.DrawSphere( Sphere?.Radius ?? 0f, Sphere?.Center ?? default, GizmoColor ),
			_ => false
		};
	}
}
