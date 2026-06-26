using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;
using MonoGameLibrary.Util;

namespace MonoGameLibrary.Graphics;

/*
public static class GraphicsManager
{
    private static GraphicsDevice _graphicsDevice;
    private static GraphicsDeviceManager _graphicsDeviceManager;

    private static SpriteVertex[] _vertices;
    private static short[] _indices;
    private static IndexBuffer _indexBuffer;
    private static VertexBuffer _vertexBuffer;
    public static Effect DefaultSpriteEffect { get; private set; }

    private static Dictionary<string, RenderPass> _renderPassesDict;
    private static List<RenderGraphNode> _renderGraphNodesList;
    private static Dictionary<string, RenderTarget2D> _nodeOutputsDict;

    private static SpriteBatch _spriteBatch;

    public static Color ClearColor;
    public static BlendState BlendState;
    public static SamplerState SamplerState;

    public static void Initialize()
    {
        ClearColor = Color.Black;
    }
    
    public static void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager, Effect defaultSpriteEffect = null)
    {
        _graphicsDevice = graphicsDevice;
        _graphicsDeviceManager = graphicsDeviceManager;

        _spriteBatch = new SpriteBatch(_graphicsDevice);

        DefaultSpriteEffect = defaultSpriteEffect ?? AssetManager.Get<Effect>("effects/spriteShader");

        ConfigureRenderPasses();
        ConfigureRenderGraphNodes();
    }

    private static void ConfigureRenderPasses()
    {
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        //};

        string path = Path.Join(PathManager.LibDataDir, "render_pass_config.json");
        string json = File.ReadAllText(path);
        var renderPassConfigsList = JsonSerializer.Deserialize<List<RenderPassConfig>>(json);

        _renderPassesDict = new Dictionary<string, RenderPass>();

        foreach (RenderPassConfig renderPassConfig in renderPassConfigsList)
        {
            string renderPassKey = renderPassConfig.Key;

            Dictionary<string, Layer> layersDict = new Dictionary<string, Layer>();
            string[] keysArray = new string[renderPassConfig.LayerNamesArray.Length];

            for (int i = 0; i < renderPassConfig.LayerNamesArray.Length; i++)
            {
                string layerKey = renderPassConfig.LayerNamesArray[i];

                layersDict.Add(layerKey, new Layer());
                keysArray[i] = layerKey;
            }

            RenderPass renderPass = new RenderPass(renderPassKey, layersDict, keysArray, renderPassConfig.Width, renderPassConfig.Height);

            _renderPassesDict.Add(renderPassKey, renderPass);
        }
    }

    

    private static void ConfigureRenderGraphNodes()
    {
        /*var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        string path = Path.Join(PathManager.LibDataDir, "render_config.json");
        string json = File.ReadAllText(path);

        _renderGraphNodesList = JsonSerializer.Deserialize<List<RenderGraphNode>>(json);
    }

    

    public static void Render()
    {
        _nodeOutputsDict = new Dictionary<string, RenderTarget2D>();

        IterateRenderPasses();
        IterateNodes();
        Clear();
    }
    
    private static void IterateRenderPasses()
    {
        foreach (KeyValuePair<string, RenderPass> kvp in _renderPassesDict)
        {
            string key = kvp.Key;
            RenderPass renderPass = kvp.Value;
            
            RenderTarget2D renderTarget = renderPass.Render();

            if (_nodeOutputsDict.ContainsKey(key))
            {
                continue;
            }
            _nodeOutputsDict.Add(key, renderTarget);
        }
    }

    private static void IterateNodes()
    {
        foreach (RenderGraphNode node in _renderGraphNodesList)
        {
            switch (node)
            {
                case EffectNode effectNode:
                    RunEffectNode(effectNode);
                    break;
                case JointNode jointNode:
                    RunJointNode(jointNode);
                    break;
                case PresentNode presentNode:
                    RunPresentNode(presentNode);
                    break;
            }
        }
    }

    private static void RunEffectNode(EffectNode effectNode)
    {
        if (!_nodeOutputsDict.ContainsKey(effectNode.InputKey))
        {
            throw new SystemException();
        }

        RenderTarget2D inputRenderTarget = _nodeOutputsDict[effectNode.InputKey];
        string[] effectKeysArray = effectNode.EffectKeysArray;

        RenderTarget2D sourceRenderTarget = inputRenderTarget;
        RenderTarget2D destinationRenderTarget = RenderTargetManager.Get(inputRenderTarget.Width, inputRenderTarget.Height);

        foreach (string effectKey in effectKeysArray)
        {
            Effect effect = NamedEffects.Get(effectKey);

            _graphicsDevice.SetRenderTarget(destinationRenderTarget);

            _spriteBatch.Begin(blendState: BlendState, samplerState: SamplerState, effect: effect);
            _spriteBatch.Draw(sourceRenderTarget, destinationRenderTarget.Bounds, Color.White);
            _spriteBatch.End();

            (sourceRenderTarget, destinationRenderTarget) = (destinationRenderTarget, sourceRenderTarget);
        }

        if (_nodeOutputsDict.ContainsKey(effectNode.OutputKey))
        {
            throw new SystemException();
        }

        _nodeOutputsDict.Add(effectNode.OutputKey, sourceRenderTarget);
    }

    public static void RunJointNode(JointNode jointNode)
    {
        RenderTarget2D[] renderTargetsArray = new RenderTarget2D[jointNode.InputKeysList.Length];

        for (int i = 0; i < jointNode.InputKeysList.Length; i++)
        {
            string key = jointNode.InputKeysList[i];

            if (!_nodeOutputsDict.ContainsKey(key))
            {
                throw new SystemException();
            }

            renderTargetsArray[i] = _nodeOutputsDict[key];
        }

        int width = renderTargetsArray[0].Width;
        int height = renderTargetsArray[0].Height;

        RenderTarget2D destinationRenderTarget = RenderTargetManager.Get(width, height);
        _graphicsDevice.SetRenderTarget(destinationRenderTarget);

        _graphicsDevice.Clear(Color.Transparent);

        _spriteBatch.Begin(blendState: BlendState, samplerState: SamplerState);
        foreach (RenderTarget2D renderTarget in renderTargetsArray)
        {
            _spriteBatch.Draw(renderTarget, destinationRenderTarget.Bounds, Color.White);
        }
        _spriteBatch.End();

        if (_nodeOutputsDict.ContainsKey(jointNode.OutputKey))
        {
            throw new SystemException();
        }

        _nodeOutputsDict.Add(jointNode.OutputKey, destinationRenderTarget);
    }

    public static void RunPresentNode(PresentNode presentNode)
    {
        if (!_nodeOutputsDict.ContainsKey(presentNode.InputKey))
        {
            throw new SystemException();
        }

        RenderTarget2D sourceRenderTarget = _nodeOutputsDict[presentNode.InputKey];

        _graphicsDevice.SetRenderTarget(null);

        _graphicsDevice.Clear(ClearColor);

        _spriteBatch.Begin(blendState: BlendState, samplerState: SamplerState);
        _spriteBatch.Draw(sourceRenderTarget, _graphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();
    }

    private static void Clear()
    {
        foreach (KeyValuePair<string, RenderPass> kvp in _renderPassesDict)
        {
            kvp.Value.Clear();
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

    public static RenderPass GetRenderPass(string key)
    {
        if (!_renderPassesDict.ContainsKey(key))
        {
            return null;
        }
        return _renderPassesDict[key];
    }

    
    

    public sealed class RenderPass
    {
        // config
        public readonly string Key;
        private readonly Dictionary<string, Layer> _layersDict;
        private readonly string[] _keysArray;

        // runtime
        public int Width {get; set;}
        public int Height {get; set;}
        public Camera Camera;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Color ClearColor;

        public RenderPass(string key, Dictionary<string, Layer> layersDict, string[] keysArray, int? width, int? height)
        {
            // config
            Key = key;
            _layersDict = layersDict;
            _keysArray = keysArray;

            // runtime
            Width = width ?? WindowManager.NativeWidth;
            Height = height ?? WindowManager.NativeHeight;
            Camera = new Camera(_graphicsDevice.Viewport);
            BlendState = BlendState.AlphaBlend;
            SamplerState = SamplerState.PointClamp;
            ClearColor = Color.Transparent;
        }

        public RenderTarget2D Render()
        {
            RenderTarget2D renderTarget = RenderTargetManager.Get(Width, Height);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);

            _graphicsDevice.SetRenderTarget(renderTarget);

            _graphicsDevice.Clear(ClearColor);

            _graphicsDevice.BlendState = BlendState;
            _graphicsDevice.SamplerStates[0] = SamplerState;
            
            Matrix view = Camera.GetViewMatrix();
            Matrix transform = view * projection;

            foreach (string key in _keysArray)
            {
                Layer layer = _layersDict[key];
                layer.Render(transform);
            }

            return renderTarget;
        }

        public void Clear()
        {
            foreach (string key in _keysArray)
            {
                Layer layer = _layersDict[key];
                layer.Clear();
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
        private List<RenderObject> _renderObjectsList = new List<RenderObject>();

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
                Effect effect = renderSprite.Effect ?? DefaultSpriteEffect;

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

            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                int primitiveCount = batch.Count * 2;

                effectPass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
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
}
*/