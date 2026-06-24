using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Content;

public static class NamedEffects
{
    private static Dictionary<string, Effect> _effectsDict;
    
    public static void Add(string key, Effect effect)
    {
        if (_effectsDict.ContainsKey(key))
        {
            return;
        }
        _effectsDict.Add(key, effect);
    }

    public static Effect Get(string key)
    {
        if (!_effectsDict.ContainsKey(key))
        {
            return null;
        }
        return _effectsDict[key];
    }
}