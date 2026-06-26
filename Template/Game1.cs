using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace Template;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphicsDeviceManager;
    private bool _isResizing;

    public Game1()
    {
        Content.RootDirectory = "Content";
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += WindowManager.OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        AssetManager.Initialize(Content);
        RenderTargetManager.Initialize();
        CursorManager.Initialize();
        InputManager.Initialize();
        WindowManager.Initialize(640, 360, Window);
        GraphicsManager.Initialize();

        InputManager.Add("exit", new InputManager.Tap(new InputManager.OrInputBind([new InputManager.Key(Keys.Escape)])));
        InputManager.Add("fullscreen", new InputManager.Tap(new InputManager.OrInputBind([new InputManager.Key(Keys.F11)])));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        AssetManager.Load();
        RenderTargetManager.Load(GraphicsDevice);
        GameLoop.NameAssets();
        WindowManager.Load(GraphicsDevice, _graphicsDeviceManager);
        GraphicsManager.Load(GraphicsDevice, _graphicsDeviceManager);

        GameLoop.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (InputManager.Get("exit"))
        {
            Exit();
        }

        if (InputManager.Get("fullscreen"))
        {
            WindowManager.ToggleFullscreen();
        }


        InputManager.Update();   

        GameLoop.Update();     

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GameLoop.Draw();

        GraphicsManager.Render();
        RenderTargetManager.ReleaseUsed();


        base.Draw(gameTime);
    }
}
