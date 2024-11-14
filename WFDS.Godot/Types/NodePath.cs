namespace WFDS.Godot.Types;

public class NodePath(string[] names, string[] subNames, bool absolute)
{
    public string[] Names { get; set; } = names;
    public string[] SubNames { get; set; } = subNames;
    public bool Absolute { get; set; } = absolute;

    public NodePath(string path) : this(path.Trim('/').Split('/'), [], path.StartsWith('/'))
    {
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