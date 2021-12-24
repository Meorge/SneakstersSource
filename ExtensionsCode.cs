using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class ExtensionsCode
{
    public static bool HasComponent <T>(this GameObject obj) where T:Component
    {
        return obj.GetComponent<T>() != null;
    }

    public static void SetAlpha(this Graphic obj, float alpha) {
        Color c = obj.color;
        c.a = alpha;
        obj.color = c;
        return;
    }

    public static void SetAlpha(this TextMeshProUGUI obj, float alpha) {
        Color c = obj.color;
        c.a = alpha;
        obj.color = c;
        return;
    }

    public static void ChangeBrightness(this Graphic obj, float amount) {
        Color c = obj.color;
        c.r *= amount;
        c.g *= amount;
        c.b *= amount;
        obj.color = c;
        return;
    }

    public static float MapToRange(float s, float a1, float a2, float b1, float b2)
    {
        float res = b1 + (s-a1)*(b2-b1)/(a2-a1);
        return Mathf.Clamp(res, b1, b2);
    }

    public static float NewLerp(float value, float a1, float a2, float b1, float b2) {
        // First, let's get the percentage within the old values:
        float percentage = Mathf.Clamp01(Mathf.InverseLerp(a1, a2, value));

        // Next, let's lerp it on the other values!
        float outVal = Mathf.Lerp(b1, b2, percentage);

        return outVal;
    }

    public static void AddOrSet(this Hashtable h, object key, object value) {
        if (h.ContainsKey(key)) h[key] = value;
        else h.Add(key, value);
        return;
    }

    public class NiceDictItem {
        public string key = "";
        public Object value;
    }

    public class NiceDict {
        private List<NiceDictItem> _items = new List<NiceDictItem>();
        public Object GetObject(string key) {
            foreach (NiceDictItem i in _items) {
                if (i.key == key) return i.value;
            }
            return null;
        }
    }
}


public static class VectorExtensions {
    public static Vector2 SetX(this Vector2 vector, float x) {
        vector.x = x;
        return vector;
    }

    public static Vector2 SetY(this Vector2 vector, float y) {
        vector.y = y;
        return vector;
    }

    public static Vector3 SetX(this Vector3 vector, float x) {
        vector.x = x;
        return vector;
    }

    public static Vector3 SetY(this Vector3 vector, float y) {
        vector.y = y;
        return vector;
    }

    public static Vector3 SetZ(this Vector3 vector, float z) {
        vector.z = z;
        return vector;
    }
}

public static class ColorExtensions {
    public static Color GetColorForChangedBrightness(this Color c, float amount) {
        c.r *= amount;
        c.g *= amount;
        c.b *= amount;
        return c;
    }

    public static Color Desaturate(this Color c, float amount) {
        float intensity = (c.r * 0.3f) + (c.g * 0.59f) + (c.b * 0.11f);

        Color totallyDesaturated = new Color(intensity, intensity, intensity);
        return Color.Lerp(c, totallyDesaturated, amount);

    }

    public static Color WithAlpha(this Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }

    public static Color FromHex(string hex) {
        try {
			string redStr = hex.Substring(0, 2);
			string greenStr = hex.Substring(2, 2);
			string blueStr = hex.Substring(4, 2);

			int redComp = int.Parse(redStr, System.Globalization.NumberStyles.HexNumber);
			int greenComp = int.Parse(greenStr, System.Globalization.NumberStyles.HexNumber);
			int blueComp = int.Parse(blueStr, System.Globalization.NumberStyles.HexNumber);

			return new Color((float)redComp / 256f, (float)greenComp / 256f, (float)blueComp / 256f);
		}
		catch {
			return Color.black;
		}
    }
}