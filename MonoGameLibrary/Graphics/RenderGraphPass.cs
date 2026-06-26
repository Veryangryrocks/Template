using System.Text.Json.Serialization;

namespace MonoGameLibrary.Graphics;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EffectPass), "raster")]
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
    [JsonPropertyName("inputs")]
    public string[] InputKeysList { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class BlitPass : RenderGraphPass
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }
    [JsonPropertyName("width")]
    public int Width { get; init; }
    [JsonPropertyName("height")]
    public int Height  { get; init; }
    [JsonPropertyName("x")]
    public int X { get; init; }
    [JsonPropertyName("y")]
    public int Y { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class PresentPass : RenderGraphPass
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }
}