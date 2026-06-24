using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Util;

namespace MonoGameLibrary.Graphics;

public static class SpriteGeometry
{
    public static void CreateQuad(
    SpriteVertex[] vertices,
    int vertexOffset,
    Vector2 position,
    Sprite sprite,
    Color color,
    Degrees rotation,
    (int, int) origin,
    Vector2 pivot,
    float scaleX,
    float scaleY,
    bool flipX,
    bool flipY)
{
    float textureWidth = sprite.Texture.Width;
    float textureHeight = sprite.Texture.Height;

    float u1 = sprite.X / textureWidth;
    float v1 = sprite.Y / textureHeight;

    float u2 = (sprite.X + sprite.Width) / textureWidth;
    float v2 = (sprite.Y + sprite.Height) / textureHeight;

    if (flipX)
    {
        (u1, u2) = (u2, u1);
    }

    if (flipY)
    {
        (v1, v2) = (v2, v1);
    }

    float originX = origin.Item1;
    float originY = origin.Item2;

    //
    // Local-space quad
    //
    Vector2 tl = new(-originX, -originY);
    Vector2 tr = new(sprite.Width - originX, -originY);
    Vector2 br = new(sprite.Width - originX, sprite.Height - originY);
    Vector2 bl = new(-originX, sprite.Height - originY);

    //
    // Scale in local space
    //
    tl *= new Vector2(scaleX, scaleY);
    tr *= new Vector2(scaleX, scaleY);
    br *= new Vector2(scaleX, scaleY);
    bl *= new Vector2(scaleX, scaleY);

    //
    // Pivot converted into local space
    //
    Vector2 localPivot =
        new Vector2(
            pivot.X - position.X,
            pivot.Y - position.Y);

    localPivot.X -= originX;
    localPivot.Y -= originY;

    localPivot *= new Vector2(scaleX, scaleY);

    //
    // Rotate around pivot
    //
    tl = Rotate(tl - localPivot, rotation) + localPivot;
    tr = Rotate(tr - localPivot, rotation) + localPivot;
    br = Rotate(br - localPivot, rotation) + localPivot;
    bl = Rotate(bl - localPivot, rotation) + localPivot;

    //
    // Move to world space
    //
    tl += position;
    tr += position;
    br += position;
    bl += position;

    vertices[vertexOffset + 0] = new SpriteVertex
    {
        Position = new Vector3(tl, 0),
        TexCoord = new Vector2(u1, v1),
        Color = color
    };

    vertices[vertexOffset + 1] = new SpriteVertex
    {
        Position = new Vector3(tr, 0),
        TexCoord = new Vector2(u2, v1),
        Color = color
    };

    vertices[vertexOffset + 2] = new SpriteVertex
    {
        Position = new Vector3(br, 0),
        TexCoord = new Vector2(u2, v2),
        Color = color
    };

    vertices[vertexOffset + 3] = new SpriteVertex
    {
        Position = new Vector3(bl, 0),
        TexCoord = new Vector2(u1, v2),
        Color = color
    };
}

    public static void CreateIndices(short[] indices, int quadIndex)
    {
        int vertexStart = quadIndex * 4;
        int indexStart = quadIndex * 6;

        indices[indexStart + 0] = (short)(vertexStart + 0);
        indices[indexStart + 1] = (short)(vertexStart + 1);
        indices[indexStart + 2] = (short)(vertexStart + 2);

        indices[indexStart + 3] = (short)(vertexStart + 0);
        indices[indexStart + 4] = (short)(vertexStart + 2);
        indices[indexStart + 5] = (short)(vertexStart + 3);
    }

    private static Vector2 Rotate(Vector2 point, Degrees rotation)
    {
        Radians radians = rotation.ToRadians();

        float cos = MathF.Cos(radians.Value);
        float sin = MathF.Sin(radians.Value);

        return new Vector2(
            point.X * cos - point.Y * sin,
            point.X * sin + point.Y * cos
        );
    }
}