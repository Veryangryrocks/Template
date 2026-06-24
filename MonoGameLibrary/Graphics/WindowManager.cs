using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public static class WindowManager
{
    private static int _nativeWidth;
    private static int _nativeHeight;
    public static int NativeWidth => _nativeWidth;
    public static int NativeHeight => _nativeHeight;
    private static int _minWidth;
    private static int _minHeight;

    private static GameWindow _gameWindow;
    private static GraphicsDevice _graphicsDevice;
    private static GraphicsDeviceManager _graphicsDeviceManager;

    private static bool _isResizing;
    private static bool _isFullscreened;
    private static int _previousWindowWidth;
    private static int _previousWindowHeight;

    public static void Initialize(int nativeWidth, int nativeHeight, GameWindow gameWindow, int? minWidth = null, int? minHeight = null)
    {
        _nativeWidth = nativeWidth;
        _nativeHeight = nativeHeight;
        _gameWindow = gameWindow;
        _minWidth = minWidth ?? nativeWidth;
        _minHeight = minHeight ?? nativeHeight;

        gameWindow.AllowUserResizing = true;
        gameWindow.ClientSizeChanged += OnClientSizeChanged;
    }

    public static void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
    {
        _graphicsDevice = graphicsDevice;
        _graphicsDeviceManager = graphicsDeviceManager;

        graphicsDeviceManager.PreferredBackBufferWidth = _nativeWidth;
        graphicsDeviceManager.PreferredBackBufferHeight = _nativeHeight;

        graphicsDeviceManager.ApplyChanges();
    }

    public static void OnClientSizeChanged(object sender, System.EventArgs e)
    {
        if (_isResizing) return;

        _isResizing = true;

        try
        {
            if (_gameWindow.ClientBounds.Width < _minWidth)
            {
                _graphicsDeviceManager.PreferredBackBufferWidth = _minWidth;
            }
            if (_gameWindow.ClientBounds.Height < _minHeight)
            {
                _graphicsDeviceManager.PreferredBackBufferHeight = _minHeight;
            }
            _graphicsDeviceManager.ApplyChanges();
        }
        finally
        {
            _isResizing = false;
        }
    }

    public static void Fullscreen()
    {
        _isFullscreened = true;
        _previousWindowWidth = _graphicsDevice.Viewport.Width;
        _previousWindowHeight = _graphicsDevice.Viewport.Height;

        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphicsDeviceManager.IsFullScreen = true;
        _graphicsDeviceManager.ApplyChanges();
    }

    public static void Unfullscreen()
    {
        _isFullscreened = false;

        _graphicsDeviceManager.PreferredBackBufferWidth = _previousWindowWidth;
        _graphicsDeviceManager.PreferredBackBufferHeight = _previousWindowHeight;
        _graphicsDeviceManager.IsFullScreen = false;
        _graphicsDeviceManager.ApplyChanges();
    }

    public static void ToggleFullscreen()
    {
        if (_isFullscreened) Unfullscreen(); else Fullscreen();
    }
}