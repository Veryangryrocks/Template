using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Util;

namespace MonoGameLibrary.Graphics;

public sealed class RenderSprite : RenderObject
{
    public enum PositionValue
    {
        TOP_LEFT,
        TOP,
        TOP_RIGHT,
        LEFT,
        CENTER,
        RIGHT,
        BOTTOM_LEFT,
        BOTTOM,
        BOTTOM_RIGHT
    }
    public readonly Sprite Sprite;
    public readonly int X, Y;
    public readonly float ScaleX, ScaleY;
    public readonly (int, int) Origin;
    public readonly Vector2 Pivot;
    public readonly Degrees Rotation;
    public readonly Color Color;
    public readonly bool FlipX, FlipY;
    public readonly Effect Effect;
    
    public RenderSprite(Sprite sprite, int x, int y, float scaleX = 1, float scaleY = 1, float depth = 0f, (int, int)? origin = null, Vector2? pivot = null, Degrees? rotation = null, Color? color = null, bool flipX = false, bool flipY = false, Effect effect = null) : base(depth)
    {
        Sprite = sprite;
        X = x;
        Y = y;
        ScaleX = scaleX;
        ScaleY = scaleY;
        Origin = origin ?? (0, 0);
        Pivot = pivot ?? Vector2.Zero;
        Rotation = rotation ?? new Degrees(0);
        Color = color ?? Color.White;
        FlipX = flipX;
        FlipY = flipY;
        Effect = effect;
    }
    public static RenderSprite FromPositions(Sprite sprite, int x, int y, float scaleX = 1, float scaleY = 1, float depth = 0, PositionValue originValue = PositionValue.TOP_LEFT, PositionValue pivotValue = PositionValue.TOP_LEFT, Degrees? rotation = null, Color? color = null, bool flipX = false, bool flipY = false, Effect effect = null)
    {
        (int, int) origin = GetOrigin(sprite.Width, sprite.Height, originValue);
        Vector2 pivot = GetPivot(new Rectangle(x, y, sprite.Width, sprite.Height), pivotValue);
        return new RenderSprite(sprite, x, y, scaleX, scaleY, depth, origin, pivot, rotation, color, flipX, flipY, effect);
    }
    public override string ToString() => $"RenderSprite [{Sprite}, {X}, {Y}, {ScaleX}, {ScaleY}, {Depth}, {Pivot}, {Rotation}, {FlipX}, {FlipY}]";
    public override int GetHashCode() => base.GetHashCode();

    private static Vector2 GetPivot(Rectangle rect, PositionValue positionValue)
    {
        return positionValue switch
        {
            PositionValue.TOP_LEFT => new Vector2(rect.Left, rect.Top),
            PositionValue.TOP => new Vector2(rect.Center.X, rect.Top),
            PositionValue.TOP_RIGHT => new Vector2(rect.Right, rect.Top),
            PositionValue.LEFT => new Vector2(rect.Left, rect.Center.Y),
            PositionValue.CENTER => new Vector2(rect.Center.X, rect.Center.Y),
            PositionValue.RIGHT => new Vector2(rect.Right, rect.Center.Y),
            PositionValue.BOTTOM_LEFT => new Vector2(rect.Left, rect.Bottom),
            PositionValue.BOTTOM => new Vector2(rect.Center.X, rect.Bottom),
            PositionValue.BOTTOM_RIGHT => new Vector2(rect.Right, rect.Bottom),
            _ => new Vector2(rect.Center.X, rect.Center.Y)
        };
    }
    private static (int, int) GetOrigin(int width, int height, PositionValue positionValue)
    {
        return positionValue switch
        {
            PositionValue.TOP_LEFT => (0, 0),
            PositionValue.TOP => (width / 2, 0),
            PositionValue.TOP_RIGHT => (width, 0),
            PositionValue.LEFT => (0, height / 2),
            PositionValue.CENTER => (width / 2, height / 2),
            PositionValue.RIGHT => (width, height / 2),
            PositionValue.BOTTOM_LEFT => (0, height),
            PositionValue.BOTTOM => (width / 2, height),
            PositionValue.BOTTOM_RIGHT => (width, height),
            _ => (0, 0)
        };
    }
}