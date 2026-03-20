using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode] 
public class MapSchemeSaver : MonoBehaviour
{
    [SerializeField] private List<Box> _mapObjects;

    [ContextMenu("SaveSchemeWood")]
    public void SaveLevelSchemeWood()
    {
        var scheme = new PositionList();
        int iterator = 0;

        foreach (var box in _mapObjects)
        {
            if (box.GetMapObjectType().Equals(MapObjectType.Wood))
            {
                var coords = box.GetRoundedCoords();
                scheme.objects.Add(new MapSchemeObject(coords.x, coords.y, box.GetMapObjectType()));
                iterator++;
            }
        }

        string json = JsonUtility.ToJson(scheme, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "wood_positions.json"), json);
        Debug.Log($"{iterator} Saved to " + Path.Combine(Application.persistentDataPath, "wood_positions.json"));
    }
}

[Serializable]
public class PositionList
{
    public List<MapSchemeObject> objects = new();
}
