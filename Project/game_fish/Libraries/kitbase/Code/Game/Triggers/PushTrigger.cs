using System.Runtime;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;

namespace GameFish;

/// <summary>
/// A trigger that affects the momentum of objects. <br />
/// Capable of creating, updating and previewing its collision.
/// <code> trigger_push </code> <code> trigger_catapult </code>
/// </summary>
[Icon( "air" )]
[Title( "Push Trigger" )]
public partial class VelocityTrigger : FilterTrigger
{
	public enum VelocityMethod
	{
		/// <summary>
		/// Doesn't apply any force.
		/// </summary>
		None,

		/// <summary>
		/// Applies all force instantly upon entering.
		/// </summary>
		[Icon( "💥" )]
		Instantaneous,

		/// <summary>
		/// Adds momentum consistently over time.
		/// </summary>
		[Icon( "🌬" )]
		Continuous,
	}

	public enum VelocityRelation
	{
		/// <summary>
		/// Always pushes/spins in the exact direction you specify.
		/// </summary>
		Absolute,

		/// <summary>
		/// The trigger's orientation offsets the force's direction.
		/// </summary>
		Trigger,

		/// <summary>
		/// Applies force relative to where the object itself is facing.
		/// </summary>
		Object,
	}

	public const string FEATURE_FORCES = "💨 Forces";

	public const string GROUP_LINEAR = "➡ Momentum";
	public const string GROUP_ANGULAR = "♻ Rotation";
	public const string GROUP_DRAG = "🐌 Drag";

	[Property, Group( GROUP_COLLIDER )]
	public override ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}
	private ColliderType _colType = ColliderType.Box;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public override BBox BoxSize
	{
		get => _boxSize;
		set { _boxSize = value; UpdateColliders(); }
	}
	private BBox _boxSize = BBox.FromPositionAndSize( Vector3.Zero, 256f );

	/// <summary>
	/// When/how linear velocity(momentum) should be added.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual VelocityMethod LinearMethod { get; set; } = VelocityMethod.Continuous;

	/// <summary>
	/// What direction linear velocity(momentum) should be added towards.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual VelocityRelation LinearRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// How much linear velocity(momentum) to add.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual Vector3 LinearVelocity { get; set; } = Vector3.Forward * 1000f;

	/// <summary>
	/// When/how angular velocity(torque/rotation) should be added.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual VelocityMethod AngularMethod { get; set; } = VelocityMethod.Continuous;

	/// <summary>
	/// What orientation angular velocity(torque/rotation) should be added around.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual VelocityRelation AngularRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// How much angular velocity(torque/rotation) to add.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual Vector3 AngularVelocity { get; set; } = default;

	/// <summary>
	/// If true: objects within this move/spin slower.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual bool Drag { get; set; } = false;

	/// <summary>
	/// Higher numbers slow down momentum more.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual float LinearDrag { get; set; } = 5f;

	/// <summary>
	/// Higher numbers slow down turning/spinning more.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual float AngularDrag { get; set; } = 5f;

	public Rotation DefaultRotation { get; } = Rotation.Identity;

	public override Color GizmoColor { get; } = Color.Cyan.Desaturate( 0.5f );

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Scene.IsValid() || this.InEditor() )
			return;

		if ( Touching is null )
			return;

		// Continuous forces/drag.
		var fixedDelta = Scene.FixedDelta;

		foreach ( var obj in Touching )
			if ( TryGetRigidbody( obj, out var rb ) )
				AddVelocity( obj, GetLinearForce( rb, instant: false ), GetAngularForce( rb, instant: false ) );
	}

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		if ( TryGetRigidbody( obj, out var rb ) )
			AddVelocity( obj, GetLinearForce( rb, instant: true ), GetAngularForce( rb, instant: true ) );
	}

	protected virtual Vector3 GetLinearForce( Rigidbody rb, in bool instant )
	{
		if ( !Scene.IsValid() || !rb.IsValid() )
			return default;

		Vector3 vel;

		if ( LinearMethod is VelocityMethod.None )
		{
			vel = default;
		}
		else if ( instant && LinearMethod is VelocityMethod.Continuous )
		{
			vel = default;
		}
		else
		{
			var r = LinearRelation switch
			{
				VelocityRelation.Absolute => DefaultRotation,
				VelocityRelation.Trigger => WorldRotation,
				VelocityRelation.Object => rb.WorldRotation,
				_ => DefaultRotation
			};

			if ( instant )
				vel = r * LinearVelocity;
			else
				vel = r * LinearVelocity * Scene.FixedDelta;
		}

		if ( !instant && Drag && LinearDrag != 0f )
			vel -= rb.Velocity.ClampLength( LinearDrag * Scene.FixedDelta );

		return vel;
	}

	protected virtual Vector3 GetAngularForce( Rigidbody rb, in bool instant )
	{
		if ( !Scene.IsValid() || !rb.IsValid() )
			return default;

		Vector3 vel;

		if ( AngularMethod is VelocityMethod.None )
		{
			vel = default;
		}
		else if ( instant && AngularMethod is VelocityMethod.Continuous )
		{
			vel = default;
		}
		else
		{
			var r = AngularRelation switch
			{
				VelocityRelation.Absolute => DefaultRotation,
				VelocityRelation.Trigger => WorldRotation,
				VelocityRelation.Object => rb.WorldRotation,
				_ => DefaultRotation
			};

			if ( instant )
				vel = r * AngularVelocity;
			else
				vel = r * AngularVelocity * Scene.FixedDelta;
		}

		if ( !instant && Drag && AngularDrag != 0f )
			vel -= rb.AngularVelocity.ClampLength( AngularDrag * Scene.FixedDelta );

		return vel;
	}

	public static bool TryGetRigidbody( GameObject obj, out Rigidbody rb )
	{
		if ( !obj.IsValid() )
		{
			rb = null;
			return false;
		}

		return obj.Components.TryGet( out rb, FindMode.EnabledInSelf );
	}

	/// <summary>
	/// Directly modifies the velocity of <paramref name="rb"/>.
	/// </summary>
	public virtual void AddVelocity( Rigidbody rb, in Vector3 linear, in Vector3 angular )
	{
		if ( !rb.IsValid() )
			return;

		rb.Velocity += linear;
		rb.AngularVelocity += angular;
	}

	/// <summary>
	/// Directly modifies velocities of the object's <see cref="Rigidbody"/>(if any).
	/// </summary>
	public void AddVelocity( GameObject obj, in Vector3 linear, in Vector3 angular )
	{
		if ( TryGetRigidbody( obj, out var rb ) )
			AddVelocity( rb, in linear, in angular );
	}
}
