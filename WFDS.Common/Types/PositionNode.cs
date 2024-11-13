using WFDS.Godot.Types;

namespace WFDS.Common.Types;

public class PositionNode
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Parent { get; set; } = string.Empty;
    public string[] Groups { get; set; } = [];
    public Transform3D Transform { get; set; } = Transform3D.Identity;
}