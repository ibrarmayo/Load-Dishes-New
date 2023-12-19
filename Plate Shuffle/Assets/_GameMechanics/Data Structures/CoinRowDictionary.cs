// DictionaryExamplesComponent.cs
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR // Editor namespaces can only be used in the editor.
using Sirenix.OdinInspector.Editor.Examples;
#endif

public class CoinRowDictionary : SerializedMonoBehaviour
{
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<PlateColor, GameObject> StringListDictionary = new Dictionary<PlateColor, GameObject>()
    {
        { PlateColor.Empty, null },
    };
}
