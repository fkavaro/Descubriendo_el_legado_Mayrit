using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NamesDatabaseSO", menuName = "Scriptable Objects/Names Database")]
public class NamesDatabaseSO : ScriptableObject
{
    [Header("Given names")]
    public List<string> maleGiven = new();
    public List<string> femaleGiven = new();

    [Header("Family names / nisbas")]
    public List<string> familyNames = new();

    readonly System.Random _rng = new();

    public string GetRandomGiven(bool female = false)
    {
        var list = female ? femaleGiven : maleGiven;
        if (list == null || list.Count == 0) return string.Empty;
        return list[_rng.Next(list.Count)];
    }

    public string GetRandomFamily()
    {
        if (familyNames == null || familyNames.Count == 0) return string.Empty;
        return familyNames[_rng.Next(familyNames.Count)];
    }
}
