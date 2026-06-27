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

        GraphicsManager.ClearColor = Color.White;

        CursorManager.Add("sword", AssetManager.Get<Texture2D>("textures/test2"), (0, 0));
        CursorManager.Set("sword");
    }

    public static void NameAssets()
    {
        
    }

    public static void Update()
    {
        _elapsedFrames += 1;
    }

    public static void Draw()
    {
        Sprite sword = new Sprite("textures/test2");

        GraphicsManager.Draw("game", "main", new RenderSprite(sword, 100, 100, 5, 5));
    }
}