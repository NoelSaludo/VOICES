using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyInventory : MonoBehaviour
{
    private readonly HashSet<string> keys = new HashSet<string>();

    public bool HasAnyKey()
    {
        return keys.Count > 0;
    }

    public bool AddKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        return keys.Add(keyId);
    }

    public bool HasKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        return keys.Contains(keyId);
    }

    public bool ConsumeKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        return keys.Remove(keyId);
    }
}
