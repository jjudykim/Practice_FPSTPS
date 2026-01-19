using System;
using System.Collections.Generic;

[Serializable]
public class MapListData
{
    public int version = 1;
    public List<MapListEntry> entries = new();
}

[Serializable]
public class MapListEntry
{
    public string key;
    public string presetFile;
}