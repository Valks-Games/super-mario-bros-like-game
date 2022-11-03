using System;

namespace Sankari;

public partial class Slime : MovingEntity
{
    public override int Gravity { get; set; } = 250;
	public override bool ClampDampen { get; set; } = false;

    private bool Jumping { get; set; }
    private GTimer JumpTimer { get; set; }
    private bool MovingForward { get; set; }
    private int WallHugTime { get; set; }

	private bool canJump;

    public override void Init()
    {
		Animations[EntityAnimationType.Idle]      = new EntityAnimationIdle(this);
		Animations[EntityAnimationType.JumpStart] = new EntityAnimationJumpStart(this);
		Animations[EntityAnimationType.JumpFall]  = new EntityAnimationJumpFall(this);

		AnimatedSprite.Animation = "idle";
		CurrentAnimation = EntityAnimationType.Idle;

        JumpTimer = new GTimer(this, nameof(OnJumpTimer), 2000, false, true);

		Label.Visible = true;
    }

    public override void UpdatePhysics()
    {
		Label.Text = "" + CurrentAnimation;

        if (IsOnFloor() && !Jumping)
        {
			Velocity = Vector2.Zero;
        }

        if (Jumping)
            Jumping = false;

        if (IsOnWall())
        {
            WallHugTime++;

            if (WallHugTime >= 50)
                MovingForward = !MovingForward;
        }

		if (IsOnFloor() && canJump)
		{
			canJump = false;
			OnJump();
			Jumping = true;
			WallHugTime = 0;

			Velocity = Velocity + new Vector2(MovingForward ? 20 : -20, -300);

			JumpTimer.Start();
		}
    }

	private void OnJumpTimer()
    {
		canJump = true;
    }
}
