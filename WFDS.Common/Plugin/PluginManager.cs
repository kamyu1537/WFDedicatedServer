﻿using System.Reflection;
using Serilog;

namespace WFDS.Common.Plugin;

public static class PluginManager
{
    private static void MakeDirectory()
    {
        var path = Path.Join(Directory.GetCurrentDirectory(), "Plugins");
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }

    public static List<Plugin> LoadPlugins()
    {
        MakeDirectory();

        var path = Path.Join(Directory.GetCurrentDirectory(), "Plugins");
        var pluginFiles = Directory.GetFiles(path, "*.dll");
        var plugins = new List<Plugin>(pluginFiles.Length);
        foreach (var pluginFile in pluginFiles)
        {
            plugins.AddRange(LoadAssembly(pluginFile));
        }

        return plugins;
    }

    private static IEnumerable<Plugin> LoadAssembly(string pluginFile)
    {
        var assembly = Assembly.LoadFrom(pluginFile);
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var plugin = LoadPlugin(type);
            if (plugin == null) continue;
            yield return plugin;
        }
    }

    private static Plugin? LoadPlugin(Type type)
    {
        if (type.IsAbstract || type.IsInterface) return null;
        if (!type.IsSubclassOf(typeof(Plugin))) return null;
        if (type.GetConstructors().All(c => c.GetParameters().Length != 0)) return null;

        var instance = Activator.CreateInstance(type);
        if (instance is not Plugin plugin) return null;
        
        Log.Logger.Information("loading plugin {Name} {Version} by {Author}", plugin.Name, plugin.Version, plugin.Author);
        plugin.Load();

        return plugin;
    }
}