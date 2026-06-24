
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public struct SpriteVertex : IVertexType
{
    public Vector3 Position;
    public Vector2 TexCoord;
    public Color Color;
    public float EffectData;

    public SpriteVertex(Vector3 position, Vector2 texCoord, Color color, float effectData = 0f)
    {
        Position = position;
        TexCoord = texCoord;
        Color = color;
        EffectData = effectData;
    }

    public static readonly VertexDeclaration VertexDeclaration = new
    (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}