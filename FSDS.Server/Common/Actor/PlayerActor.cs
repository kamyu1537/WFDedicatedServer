namespace FSDS.Server.Common.Actor;

public class PlayerActor : BaseActor
{
    public string Name { get; set; } = string.Empty;

    public override void OnCreated()
    {
        ActorType = "player";
    }
}