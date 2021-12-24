using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Simple String Dictionary", menuName = "Simple String Dictionary", order = 1)]

public class SimpleStringDictionary : ScriptableObject
{
    public List<SimpleStringDictionaryItem> items = new List<SimpleStringDictionaryItem>();

    public string GetValueForKey(string key) {
        foreach (SimpleStringDictionaryItem i in items) {
            if (i.keys.Contains(key)) return i.value;
        }
        return null;
    }
}

[System.Serializable]
public class SimpleStringDictionaryItem
{
    public List<string> keys = new List<string>();
    public string value = "";
}
