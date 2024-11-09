namespace WFDS.Godot.Types;

public class NodePath
{
    public string[] Names { get; set; }
    public string[] SubNames { get; set; }
    public bool Absolute { get; set; }

    public NodePath(string path)
    {
        Absolute = path.StartsWith('/');
        Names = path.Trim('/').Split('/');
        SubNames = [];
    }

    public NodePath(string[] names, string[] subNames, bool absolute)
    {
        Names = names;
        SubNames = subNames;
        Absolute = absolute;
    }

    public override string ToString()
    {
        var path = string.Join("/", Names);
        if (SubNames.Length > 0)
        {
            path += ":" + string.Join(":", SubNames);
        }
        return Absolute ? "/" + path : path;
    }
}