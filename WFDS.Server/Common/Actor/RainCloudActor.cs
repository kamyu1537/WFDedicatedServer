using WFDS.Godot.Types;

namespace WFDS.Server.Common.Actor;

public class RainCloudActor : BaseActor
{
    private const float Speed = 0.17f;
    private float _direction;

    public override void OnCreated()
    {
        ActorType = "raincloud";

        var center = (Position - new Vector3(30, 40, -50)).Normalized();
        _direction = new Vector2(center.X, center.Z).Angle();
    }

    public override void OnUpdate(double delta)
    {
        var vel = new Vector2(1, 0).Rotate(_direction) * Speed;
        Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
    }
}