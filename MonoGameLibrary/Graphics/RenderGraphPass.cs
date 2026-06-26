using System.Text.Json.Serialization;

namespace MonoGameLibrary.Graphics;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RasterPass), "raster")]
[JsonDerivedType(typeof(EffectPass), "effect")]
[JsonDerivedType(typeof(CompositePass), "composite")]
[JsonDerivedType(typeof(BlitPass), "blit")]
[JsonDerivedType(typeof(PresentPass), "present")]
public abstract class RenderGraphPass;

public sealed class RasterPass : RenderGraphPass
{
    [JsonPropertyName("layers")]
    public string[] LayerKeysArray { get; init; }
    [JsonPropertyName("width")]
    public int Width { get; init; }
    [JsonPropertyName("height")]
    public int Height { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class EffectPass : RenderGraphPass
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }

    [JsonPropertyName("effects")]
    public string[] EffectKeysArray { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class CompositePass : RenderGraphPass
{
    [JsonPropertyName("input_back")]
    public string InputKeyBack{ get; init; }
    [JsonPropertyName("input_front")]
    public string InputKeyFront { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class BlitPass : RenderGraphPass
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }
    [JsonPropertyName("width")]
    public int TargetWidth { get; init; }
    [JsonPropertyName("height")]
    public int TargetHeight  { get; init; }
    [JsonPropertyName("rect_width")]
    public int RectWidth { get; init; }
    [JsonPropertyName("rect_height")]
    public int RectHeight  { get; init; }
    [JsonPropertyName("rect_x")]
    public int RectX { get; init; }
    [JsonPropertyName("rect_y")]
    public int RectY { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class PresentPass : RenderGraphPass
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }
}