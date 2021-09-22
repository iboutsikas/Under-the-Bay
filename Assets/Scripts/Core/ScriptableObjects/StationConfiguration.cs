using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StationDescription
{
    public string StationName;
    public string FriendlyName;
    public Sprite SelectedSprite;
    public bool IsDefaultStation;
}

[CreateAssetMenu(fileName = "Station Configuration", menuName = "Settings/Station Configuration", order = 2)]
public class StationConfiguration : ScriptableObject
{
    public List<StationDescription> Stations;
}
