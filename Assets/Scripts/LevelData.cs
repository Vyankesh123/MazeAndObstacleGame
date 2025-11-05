using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Maze/LevelList")]
public class LevelData : ScriptableObject
{
    [TextArea(2, 40)] public List<string> levels = new();
}
