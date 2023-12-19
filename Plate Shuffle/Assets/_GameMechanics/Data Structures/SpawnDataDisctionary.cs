// DictionaryExamplesComponent.cs
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR // Editor namespaces can only be used in the editor.
using Sirenix.OdinInspector.Editor.Examples;
#endif

public class SpawnDataDisctionary : SerializedMonoBehaviour
{
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<PlateColor, Plate> PlatesData = new Dictionary<PlateColor, Plate>()
    {
        { PlateColor.Empty, null },
    };

    public Plate GetPlate(PlateColor color)
    {
        return PlatesData[color];
    }
}
