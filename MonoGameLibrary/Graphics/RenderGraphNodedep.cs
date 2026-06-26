/*
using System.Text.Json.Serialization;

namespace MonoGameLibrary.Graphics;

public sealed class RenderPassConfig
{
    [JsonPropertyName("key")]
    public string Key {get; init;}
    [JsonPropertyName("layers")]
    public string[] LayerNamesArray {get; init;}
    [JsonPropertyName("width")]
    public int? Width {get; init;}
    [JsonPropertyName("height")]
    public int? Height {get; init;}
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EffectNode), "effect")]
[JsonDerivedType(typeof(JointNode), "joint")]
[JsonDerivedType(typeof(PresentNode), "present")]
public abstract class RenderGraphNode {}
public sealed class EffectNode : RenderGraphNode
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }

    [JsonPropertyName("effects")]
    public string[] EffectKeysArray { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class JointNode : RenderGraphNode
{
    [JsonPropertyName("inputs")]
    public string[] InputKeysList { get; init; }
    [JsonPropertyName("output")]
    public string OutputKey { get; init; }
}

public sealed class PresentNode : RenderGraphNode
{
    [JsonPropertyName("input")]
    public string InputKey { get; init; }
}
*/