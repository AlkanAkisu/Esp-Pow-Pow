using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static int GetMin<T>(this List<T> list, Func<T, float> func, out T minVal)
    {
        var i = 0;
        var minIndex = 0;
        var minValue = float.MaxValue;
        foreach (var t in list)
        {
            var val = func(t);
            if (val < minValue)
            {
                minValue = val;
                minIndex = i;
            }
            i++;
        }
        minVal = list[minIndex];
        return minIndex;
    }
    
    public static int GetMax<T>(this List<T> list, Func<T, float> func, out T maxVal)
    {
        var i = 0;
        var maxIndex = 0;
        var maxValue = float.MinValue;
        foreach (var t in list)
        {
            var val = func(t);
            if (val > maxValue)
            {
                maxValue = val;
                maxIndex = i;
            }
            i++;
        }
        maxVal = list[maxIndex];
        return maxIndex;
    }

    public static List<T> Copy<T>(this List<T> list) => list.Select(e => e).ToList();

    public static void DrawRect(Rect rect, float duration, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y), color, duration);
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height), color, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y),
            color, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height),
            new Vector3(rect.x, rect.y + rect.height), color, duration);
    }
    
    public static Vector3 ChangeVector(this Vector3 vect, float x = Mathf.Infinity, float y = Mathf.Infinity, float z = Mathf.Infinity)
    {
        return new Vector3(float.IsPositiveInfinity(x) ? vect.x : x, float.IsPositiveInfinity(y) ? vect.y : y, float.IsPositiveInfinity(z) ? vect.z : z);
    }
    public static Vector2 ChangeVector(this Vector2 vect, float x = Mathf.Infinity, float y = Mathf.Infinity)
    {
        return new Vector2(float.IsPositiveInfinity(x) ? vect.x : x, float.IsPositiveInfinity(y) ? vect.y : y);
    }
    
    
}