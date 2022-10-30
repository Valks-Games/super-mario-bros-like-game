namespace Sankari;

public partial class BasicEnemy : Entity, IEnemy, IEntity, IEntityMovement
{
	[Export] public float Speed { get; set; } = 40;
	[Export] public bool Active { get; set; } = true;
	[Export] public bool StartWalkingRight { get; set; }
	[Export] public bool DontCollideWithWall { get; set; }
	[Export] public bool FallOffCliff { get; set; }

	public override int Gravity { get; set; } = 30000;
	public AnimatedSprite2D AnimatedSprite { get; set; }
	private bool MovingForward { get; set; }

	private RayCast2D RayCastWallLeft { get; set; }
	private RayCast2D RayCastWallRight { get; set; }
	private RayCast2D RayCastCliffLeft { get; set; }
	private RayCast2D RayCastCliffRight { get; set; }
	public int GroundAcceleration { get; set; } = 50;
	public Window Tree { get; set; }

	public override void _Ready()
	{
		AnimatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Activate();

		RayCastWallLeft = PrepareRaycast("Wall Checks/Left");
		RayCastWallRight = PrepareRaycast("Wall Checks/Right");
		RayCastCliffLeft = PrepareRaycast("Cliff Checks/Left");
		RayCastCliffRight = PrepareRaycast("Cliff Checks/Right");

		RayCast2DGroundChecks.Add(RayCastCliffRight);
		RayCast2DGroundChecks.Add(RayCastCliffLeft);

		if (FallOffCliff)
		{
			RayCastCliffLeft.Enabled = false;
			RayCastCliffRight.Enabled = false;
		}

		if (DontCollideWithWall)
		{
			RayCastWallLeft.Enabled = false;
			RayCastWallRight.Enabled = false;
		}

		if (StartWalkingRight)
		{
			MovingForward = !MovingForward;
			AnimatedSprite.FlipH = true;
		}

		if (!Active)
		{
			AnimatedSprite.Stop();
			AnimatedSprite.Frame = 0;
			SetPhysicsProcess(false);
		}

		FloorStopOnSlope = false;

		Commands[EntityCommandType.Movement] = new EntityCommandMovement(this);
		
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		Delta = (float)delta;
		var velocity = new Vector2(0,0);

		if (MovingForward)
		{
			// Move forwards
			MoveDir = Vector2.Left;
			velocity.x += Speed;

			// If the entity is set to collide with a wall then change directions
			// when touching a wall
			if (!DontCollideWithWall && IsRaycastColliding(RayCastWallRight))
				ChangeDirection();

			// If the entity is set to not fall off a cliff and there is no ground
			// to the right in front of the entity then assume there is a cliff and
			// prevent the entity from falling off the cliff by changing directions
			if (!FallOffCliff && !IsRaycastColliding(RayCastCliffRight))
				ChangeDirection();
		}
		else
		{
			// Move backwards
			MoveDir = Vector2.Right;
			velocity.x -= Speed;

			// If the entity is set to collide with a wall then change directions
			// when touching a wall
			if (!DontCollideWithWall && IsRaycastColliding(RayCastWallLeft))
				ChangeDirection();

			// If the entity is set to not fall off a cliff and there is no ground
			// to the left in front of the entity then assume there is a cliff and
			// prevent the entity from falling off the cliff by changing directions
			if (!FallOffCliff && !IsRaycastColliding(RayCastCliffLeft))
				ChangeDirection();
		}

		Velocity = velocity;

		base._PhysicsProcess(delta);
	}

	public void Activate()
	{
		SetPhysicsProcess(true);
		AnimatedSprite.Frame = GD.RandRange(0, AnimatedSprite.Frames.GetFrameCount("default"));
		AnimatedSprite.SpeedScale = 1 + (Speed * 0.002f);
		AnimatedSprite.Play();
	}

	public void Deactivate()
	{
		SetPhysicsProcess(false);
		AnimatedSprite.Stop();
	}

	public void Destroy()
	{
		QueueFree();
	}

	private void ChangeDirection()
	{
		MovingForward = !MovingForward;
		AnimatedSprite.FlipH = !AnimatedSprite.FlipH;
	}

	private bool IsRaycastColliding(RayCast2D raycast)
	{
		var collider = raycast.GetCollider() as Node;

		if (collider != null && collider.IsInGroup("Tileset"))
		{
			return true;
		}

		return false;
	}

	private void _on_enemy_area_entered(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			var player = area.GetParent<Player>();
			player.TakenDamage(player.GetCollisionSide(area), 1);
			player.InDamageZone = true;
		}
	}

	private void _on_enemy_area_exited(Area2D area) 
	{
		if (area.IsInGroup("Player"))
		{
			var player = area.GetParent<Player>();
			player.InDamageZone = false;
		}	
	}
}
