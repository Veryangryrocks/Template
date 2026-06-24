using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Util;

namespace Template;

public static class GameLoop
{
    private static int _elapsedFrames;
    public static void Initialize()
    {
        _elapsedFrames = 0;

        CursorManager.Add("sword", AssetManager.Get<Texture2D>("textures/test2"), (0, 0));
        CursorManager.Set("sword");
    }

    public static void NameAssets()
    {
        
    }

    public static void Update()
    {
        _elapsedFrames += 1;

        GraphicsManager.RenderPass rp = GraphicsManager.GetRenderPass("main");
    }

    public static void Draw()
    {
        Sprite leftHalf = new Sprite("textures/test", 0, 0, 32, 64);
        Sprite sword = new Sprite("textures/test2");

        GraphicsManager.Layer layer = GraphicsManager.GetRenderPass("main").GetLayer("main");

        
        layer.Add(GraphicsManager.RenderSprite.FromPositions(leftHalf, 100, 100, depth: 1.0f, color: new(255, 0, 255, 100), pivotValue: GraphicsManager.RenderSprite.PositionValue.TOP_LEFT));
        layer.Add(GraphicsManager.RenderSprite.FromPositions(sword, 0, 0, 4, 4, pivotValue: GraphicsManager.RenderSprite.PositionValue.CENTER));
    }
}