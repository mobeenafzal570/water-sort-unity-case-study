using System;
using UnityEngine;

// Distinct liquid hues and separated light/dark neutrals.
// Level data uses these names consistently for layers, locks, receivers and effects.
public static class GamePalette
{
    public static readonly string[] Names = { "Red", "Gold", "Emerald Green", "Blue",
        "Orange", "Purple", "Cyan blue", "Ivory", "Charcoal", "Silver" };
    private static readonly Color32[] Colors = {
        new Color32(232, 35, 57, 255), new Color32(255, 215, 0, 255),
        new Color32(0, 157, 75, 255), new Color32(46, 76, 222, 255),
        new Color32(255, 131, 24, 255), new Color32(173, 26, 184, 255),
        // Keep the serialized Ivory ID so existing levels and locks still match.
        // Its liquid, streams and effects now render in deep teal.
        new Color32(0, 207, 221, 255), new Color32(0, 112, 112, 255),
        new Color32(35, 43, 55, 255), new Color32(155, 169, 183, 255)
    };
    private static Material liquidMaterial;
    public static Material LiquidMaterial => liquidMaterial != null ? liquidMaterial :
        (liquidMaterial = Resources.Load<Material>("LiquidPaletteUI"));
    private static Material continuousLiquidMaterial;
    public static Material ContinuousLiquidMaterial => continuousLiquidMaterial != null ? continuousLiquidMaterial :
        (continuousLiquidMaterial = Resources.Load<Material>("ContinuousLiquidUI"));
    public static bool TryGet(string name, out Color color)
    {
        for (int i = 0; i < Names.Length; i++)
            if (string.Equals(name, Names[i], StringComparison.OrdinalIgnoreCase))
            { color = Colors[i]; return true; }
        color = Color.white;
        return false;
    }
    public static Color Get(string name) { TryGet(name, out Color color); return color; }
}
