using System.Collections.Generic;

namespace Universum.Cache;

public static class ObjectHolder {
    private static readonly Dictionary<int, World.ObjectHolder> OBJECT_HOLDERS = new();

    public static void Add(World.ObjectHolder objectHolder) => OBJECT_HOLDERS[objectHolder.Tile] = objectHolder;

    public static void Remove(World.ObjectHolder objectHolder) => OBJECT_HOLDERS.Remove(objectHolder.Tile);

    public static void Clear() => OBJECT_HOLDERS.Clear();

    public static bool Exists(int tile) => OBJECT_HOLDERS.ContainsKey(tile);

    public static World.ObjectHolder Get(int tile) => OBJECT_HOLDERS.GetValueOrDefault(tile);
}
