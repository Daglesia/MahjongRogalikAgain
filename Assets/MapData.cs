using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapNode
{
    public int Id;
    public int Column;
    public int Lane;
    public LocationType Type;
    public Vector2 Position;
    public List<int> NextIds = new List<int>();
}

[Serializable]
public class MapData
{
    public int Seed;
    public List<MapNode> Nodes = new List<MapNode>();
}