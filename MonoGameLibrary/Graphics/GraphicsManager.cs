

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;

namespace MonoGameLibrary.Graphics;

public static class GraphicsManager
{
    // resources
    private static GraphicsDevice _graphicsDevice;
    private static Effect _defaultSpriteEffect;
    private static List<RenderGraphPass> _renderGraphPassesList;
    private static Dictionary<string, RenderTarget2D> _passOutputsDict;
    private static Dictionary<string, RasterPassWrapper> _rasterPassWrappersDict;

    private static SpriteVertex[] _vertices;
    private static short[] _indices;
    private static IndexBuffer _indexBuffer;
    private static VertexBuffer _vertexBuffer;
    private static SpriteBatch _spriteBatch;

    // runtime
    public static Color ClearColor { get; set; }
    public static SamplerState DefaultSamplerState { get; set; }
    public static BlendState DefaultBlendState { get; set; }

    public static void Initialize()
    {
        ClearColor = Color.White;
        DefaultSamplerState = SamplerState.PointClamp;
        DefaultBlendState = BlendState.AlphaBlend;
    }

    public static void Load(GraphicsDevice graphicsDevice, Effect defaultSpriteEffect, string renderGraphJson)
    {
        _graphicsDevice = graphicsDevice;
        _defaultSpriteEffect = defaultSpriteEffect;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        LoadRenderGraph(renderGraphJson);
    }

    public static void LoadRenderGraph(string json)
    {
        _renderGraphPassesList = JsonSerializer.Deserialize<List<RenderGraphPass>>(json);
        LoadRasterPassWrappers();
    }

    private static void LoadRasterPassWrappers()
    {
        _rasterPassWrappersDict = new Dictionary<string, RasterPassWrapper>();

        List<RasterPass> rasterPassesList = _renderGraphPassesList.OfType<RasterPass>().ToList();

        foreach (RasterPass rasterPass in rasterPassesList)
        {
            string key = rasterPass.OutputKey;
            Dictionary<string, Layer> layersDict = new Dictionary<string, Layer>();
            string[] layerKeysArray = rasterPass.LayerKeysArray;
            int width = rasterPass.Width;
            int height = rasterPass.Height;

            foreach (string layerKey in layerKeysArray)
            {
                layersDict.Add(layerKey, new Layer());
            }

            _rasterPassWrappersDict.Add(key, new RasterPassWrapper(layersDict, layerKeysArray, width, height));
        }
    }

    public static void Render()
    {
        _passOutputsDict = new Dictionary<string, RenderTarget2D>();

        foreach (RenderGraphPass renderGraphPass in _renderGraphPassesList)
        {
            switch (renderGraphPass)
            {
                case RasterPass renderPass:
                    ExecuteRasterPass(renderPass);
                    continue;
                case EffectPass effectPass:
                    ExecuteEffectPass(effectPass);
                    continue;
                case CompositePass compositePass:
                    ExecuteCompositePass(compositePass);
                    continue;
                case BlitPass blitPass:
                    ExecuteBlitPass(blitPass);
                    continue;
                case PresentPass presentPass:
                    ExecutePresentPass(presentPass);
                    continue;
            }
        }

        RenderTargetManager.ReleaseUsed();
        Clear();
    }

    private static void ExecuteRasterPass(RasterPass rasterPass)
    {
        string key = rasterPass.OutputKey;
        RasterPassWrapper rasterPassWrapper = _rasterPassWrappersDict[key];
        
        if (rasterPassWrapper == null)
        {
            throw new System.Exception();
        }

        RenderTarget2D renderTarget = rasterPassWrapper.Render();
        _passOutputsDict.Add(key, renderTarget);
    }

    private static void ExecuteEffectPass(EffectPass effectPass)
    {
        if (!_passOutputsDict.ContainsKey(effectPass.InputKey))
        {
            throw new System.Exception();
        }

        RenderTarget2D inputRenderTarget = _passOutputsDict[effectPass.InputKey];
        RenderTarget2D sourceRenderTarget = inputRenderTarget;
        RenderTarget2D destinationRenderTarget = RenderTargetManager.Get(inputRenderTarget.Width, inputRenderTarget.Height);

        string[] effectKeysArray = effectPass.EffectKeysArray;

        foreach (string effectKey in effectKeysArray)
        {
            Effect effect = NamedEffects.Get(effectKey);

            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _spriteBatch.Begin(blendState: DefaultBlendState, samplerState: DefaultSamplerState, effect: effect);
            _spriteBatch.Draw(sourceRenderTarget, destinationRenderTarget.Bounds, Color.White);
            _spriteBatch.End();

            (sourceRenderTarget, destinationRenderTarget) = (destinationRenderTarget, sourceRenderTarget);
        }

        if (_passOutputsDict.ContainsKey(effectPass.OutputKey))
        {
            throw new System.Exception();
        }

        _passOutputsDict.Add(effectPass.OutputKey, sourceRenderTarget);
    }

    private static void ExecuteCompositePass(CompositePass compositePass)
    {
        if (!_passOutputsDict.ContainsKey(compositePass.InputKeyBack) || !_passOutputsDict.ContainsKey(compositePass.InputKeyFront))
        {
            throw new System.Exception();
        }

        RenderTarget2D backRenderTarget = _passOutputsDict[compositePass.InputKeyBack];
        RenderTarget2D frontRenderTarget =  _passOutputsDict[compositePass.InputKeyFront];

        if (backRenderTarget.Width != frontRenderTarget.Width || backRenderTarget.Height != frontRenderTarget.Height)
        {
            throw new System.Exception();
        }

        _graphicsDevice.SetRenderTarget(backRenderTarget);

        _spriteBatch.Begin(blendState: DefaultBlendState, samplerState: DefaultSamplerState);
        _spriteBatch.Draw(frontRenderTarget, backRenderTarget.Bounds, Color.White);
        _spriteBatch.End();

        if (_passOutputsDict.ContainsKey(compositePass.OutputKey))
        {
            throw new System.Exception();
        }

        _passOutputsDict.Add(compositePass.OutputKey, backRenderTarget);
    }

    private static void ExecuteBlitPass(BlitPass blitPass)
    {
        if (!_passOutputsDict.ContainsKey(blitPass.InputKey))
        {
            throw new System.Exception();
        }

        Rectangle rect = new Rectangle(blitPass.RectX, blitPass.RectY, blitPass.RectWidth, blitPass.RectHeight);

        RenderTarget2D sourceRenderTarget = _passOutputsDict[blitPass.InputKey];
        RenderTarget2D destinationRenderTarget = RenderTargetManager.Get(blitPass.TargetWidth, blitPass.TargetHeight);

        _graphicsDevice.SetRenderTarget(destinationRenderTarget);
        _graphicsDevice.Clear(Color.Transparent);

        _spriteBatch.Begin(blendState: DefaultBlendState, samplerState: DefaultSamplerState);
        _spriteBatch.Draw(sourceRenderTarget, rect, Color.White);
        _spriteBatch.End();

        if (_passOutputsDict.ContainsKey(blitPass.OutputKey))
        {
            throw new System.Exception();
        }

        _passOutputsDict.Add(blitPass.OutputKey, destinationRenderTarget);
    }

    private static void ExecutePresentPass(PresentPass presentPass)
    {
        if (!_passOutputsDict.ContainsKey(presentPass.InputKey))
        {
            throw new System.Exception();
        }

        Console.WriteLine("present pass");

        RenderTarget2D renderTarget = _passOutputsDict[presentPass.InputKey];
        
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(ClearColor);

        float scale = MathF.Min(
            (float)_graphicsDevice.Viewport.Width / renderTarget.Width, 
            (float)_graphicsDevice.Viewport.Height / renderTarget.Height);

        Rectangle dest = new(
            (int)((_graphicsDevice.Viewport.Width - renderTarget.Width * scale) * 0.5f),
            (int)((_graphicsDevice.Viewport.Height - renderTarget.Height * scale) * 0.5f),
            (int)(renderTarget.Width * scale),
            (int)(renderTarget.Height * scale));

        _spriteBatch.Begin(blendState: DefaultBlendState, samplerState: DefaultSamplerState);
        _spriteBatch.Draw(renderTarget, dest, Color.White);
        _spriteBatch.End();
    }

    public sealed class RasterPassWrapper
    {
        // resources
        private readonly Dictionary<string, Layer> _layersDict;
        private readonly string[] _layerKeysArray;
        public readonly int Width;
        public readonly int Height;

        // runtime
        public Camera Camera { get; set; }

        public RasterPassWrapper(Dictionary<string, Layer> layersDict, string[] layerKeysArray, int width, int height)
        {
            // resources
            _layersDict = layersDict;
            _layerKeysArray = layerKeysArray;
            Width = width;
            Height = height;

            // runtime
            Camera = new Camera(new Rectangle(0, 0, width, height));
        }

        public RenderTarget2D Render()
        {
            Matrix view = Camera.GetViewMatrix();
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
            Matrix transform = projection;

            RenderTarget2D renderTarget = RenderTargetManager.Get(Width, Height);
            
            _graphicsDevice.SetRenderTarget(renderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            _graphicsDevice.SamplerStates[0] = DefaultSamplerState;
            _graphicsDevice.BlendState = DefaultBlendState;
            _graphicsDevice.Viewport = new Viewport(
    0,
    0,
    renderTarget.Width,
    renderTarget.Height);

            foreach (string key in _layerKeysArray)
            {
                _layersDict[key].Render(transform);
            }

            return renderTarget;
        }

        public void Clear()
        {
            foreach (string key in _layerKeysArray)
            {
                _layersDict[key].Clear();
            }
        }

        public Layer GetLayer(string key)
        {
            if (!_layersDict.ContainsKey(key))
            {
                return null;
            }
            return _layersDict[key];
        }
    }

    public sealed class Layer
    {
        private List<RenderObject> _renderObjectsList;

        public Layer()
        {
            _renderObjectsList = new List<RenderObject>();
        }

        public void Render(Matrix transform)
        {
            List<RenderSprite> renderSpritesList = _renderObjectsList.OfType<RenderSprite>().ToList();
            renderSpritesList.Sort((a, b) => b.Depth.CompareTo(a.Depth));

            if (renderSpritesList.Count == 0)
            {
                return;
            }

            Texture2D currentTexture = null;
            Effect currentEffect = null;

            List<RenderSprite> batch = new List<RenderSprite>();

            foreach (RenderSprite renderSprite in renderSpritesList)
            {
                Texture2D texture = renderSprite.Sprite.Texture;
                Effect effect = renderSprite.Effect ?? _defaultSpriteEffect;

                if (texture != currentTexture || effect != currentEffect)
                {
                    Flush(batch, transform, currentEffect);
                    batch.Clear();

                    currentTexture = texture;
                    currentEffect = effect;
                }

                batch.Add(renderSprite);
            }

            Flush(batch, transform, currentEffect);
        }

        private void Flush(List<RenderSprite> batch, Matrix transform, Effect effect)
        {
            if (batch.Count == 0)
            {
                return;
            }

            _vertices = new SpriteVertex[batch.Count * 4];
            _indices = new short[batch.Count * 6];
            ResizeBuffers();

            for (int i = 0; i < batch.Count; i++)
            {
                RenderSprite renderSprite = batch[i];

                SpriteGeometry.CreateQuad(_vertices, i * 4, new Vector2(renderSprite.X, renderSprite.Y), renderSprite.Sprite, renderSprite.Color, renderSprite.Rotation, renderSprite.Origin, renderSprite.Pivot, renderSprite.ScaleX, renderSprite.ScaleY, renderSprite.FlipX, renderSprite.FlipY);
                SpriteGeometry.CreateIndices(_indices, i);
            }

            _vertexBuffer.SetData(_vertices);
            _indexBuffer.SetData(_indices);

            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            effect.Parameters["MatrixTransform"].SetValue(transform);
            effect.Parameters["SpriteTexture"].SetValue(batch[0].Sprite.Texture);

            for (int i = 0; i < _indices.Length; i++)
    Console.WriteLine(_indices[i]);

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
_graphicsDevice.DepthStencilState = DepthStencilState.None;
_graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                int primitiveCount = batch.Count * 2;

                Console.WriteLine(effect.Name);
Console.WriteLine(effect.CurrentTechnique.Name);
Console.WriteLine("here");
Console.WriteLine(effect.Parameters["MatrixTransform"]);
Console.WriteLine(effect.Parameters["SpriteTexture"]);
Console.WriteLine(effect.CurrentTechnique.Name);

Console.WriteLine("ACTUALLY HERE");
Console.WriteLine(_vertexBuffer);
Console.WriteLine(_graphicsDevice.Indices);

                effectPass.Apply();
                //_graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);

                _graphicsDevice.DrawIndexedPrimitives(
    PrimitiveType.TriangleList,
    baseVertex: 0,
    minVertexIndex: 0,
    numVertices: batch.Count * 4,
    startIndex: 0,
    primitiveCount: batch.Count * 2);
            }
        }

        private static void ResizeBuffers()
        {
            if (_vertexBuffer == null || _vertexBuffer.VertexCount != _vertices.Length)
            {
                if (_vertexBuffer != null)
                {
                    _vertexBuffer.Dispose();
                }
                _vertexBuffer = new VertexBuffer(_graphicsDevice, SpriteVertex.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || _indexBuffer.IndexCount != _indices.Length)
            {
                if (_indexBuffer != null)
                {
                    _indexBuffer.Dispose();
                }
                _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
            }
        }

        public void Clear()
        {
            _renderObjectsList.Clear();
        }

        public void Add(RenderObject renderObject)
        {
            _renderObjectsList.Add(renderObject);
        }
    }

    public static void Draw(string rasterPassWrapperKey, string layerKey, RenderObject renderObject)
    {
        RasterPassWrapper rasterPassWrapper = GetRasterPassWrapper(rasterPassWrapperKey);

        if (rasterPassWrapper == null)
        {
            return;
        }

        Layer layer = rasterPassWrapper.GetLayer(layerKey);

        if (layer == null)
        {
            return;
        }

        layer.Add(renderObject);
    }

    public static RasterPassWrapper GetRasterPassWrapper(string key)
    {
        if (!_rasterPassWrappersDict.ContainsKey(key))
        {
            return null;
        }
        return _rasterPassWrappersDict[key];
    }

    private static void Clear()
    {
        foreach (KeyValuePair<string, RasterPassWrapper> kvp in _rasterPassWrappersDict)
        {
            kvp.Value.Clear();
        }
    }
}