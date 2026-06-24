/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;
using MonoGameLibrary.Util;


namespace MonoGameLibrary.Graphics;

public static class GraphicsManager
{

    private static GraphicsDevice _graphicsDevice;
    private static GraphicsDeviceManager _graphicsDeviceManager;

    private static Dictionary<string, RenderPass> _renderPassesDict;
    private static List<string> _renderPassKeysList;

    private static Dictionary<string, RenderTarget2D> _presentedRenderTargetsDict;
    private static List<string> _presentedRenderTargetKeysList;


    private static SpriteVertex[] _vertices;
    private static short[] _indices;
    private static IndexBuffer _indexBuffer;
    private static VertexBuffer _vertexBuffer;
    private static Effect _spriteEffect;

    public static Color ClearColor;

    public static void Initialize()
    {
        _presentedRenderTargetsDict = new Dictionary<string, RenderTarget2D>();
        _presentedRenderTargetKeysList = new List<string>();

        ClearColor = Color.Black;
    }

    private static void ConfigureRenderPasses(List<RenderPassConfig> renderPassConfigsList)
    {
        _renderPassesDict = new Dictionary<string, RenderPass>();
        _renderPassKeysList = new List<string>();

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
            _renderPassKeysList.Add(renderPassKey);
        }
    }

    public static void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
    {
        _graphicsDevice = graphicsDevice;
        _graphicsDeviceManager = graphicsDeviceManager;

        _spriteEffect = AssetManager.Get<Effect>("effects/spriteShader");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        string path = Path.Join(PathManager.LibDataDir, "render_pass_configs.json");
        string json = File.ReadAllText(path);
        var renderPassConfigsList = JsonSerializer.Deserialize<List<RenderPassConfig>>(json, options);

        ConfigureRenderPasses(renderPassConfigsList);
    }
    public static void Render()
    {
        foreach (string key in _renderPassKeysList)
        {
            _renderPassesDict[key].Render();
        }

        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(ClearColor);

        foreach (string key in _presentedRenderTargetKeysList)
        {
            RenderTarget2D renderTarget = _presentedRenderTargetsDict[key];
            FlushRenderTarget(renderTarget);
        }
    }

    public static void Clear()
    {
        foreach (string key in _renderPassKeysList)
        {
            _renderPassesDict[key].Clear();
        }
    }

    public abstract class RenderObject
    {
        public readonly float Depth;
        protected RenderObject(float depth)
        {
            Depth = depth;
        }
    }

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
            Effect = effect ?? _spriteEffect;
        }
        public static RenderSprite FromPositions(Sprite sprite, int x, int y, float scaleX = 1, float scaleY = 1, float depth = 0, PositionValue originValue = PositionValue.TOP_LEFT, PositionValue pivotValue = PositionValue.TOP_LEFT, Degrees? rotation = null, Color? color = null, bool flipX = false, bool flipY = false, Effect effect = null)
        {
            (int, int) origin = GetOrigin(sprite.Width, sprite.Height, originValue);
            Vector2 pivot = GetPivot(new Rectangle(x, y, sprite.Width, sprite.Height), pivotValue);
            return new RenderSprite(sprite, x, y, scaleX, scaleY, depth, origin, pivot, rotation, color, flipX, flipY, effect);
        }
        public override string ToString() => $"[{Sprite}, {X}, {Y}, {ScaleX}, {ScaleY}, {Depth}, {Pivot}, {Rotation}, {FlipX}, {FlipY}, {Effect}]";
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

    public sealed class RenderPass
    {
        // config
        public readonly string Key;
        private readonly Dictionary<string, Layer> _layersDict;
        private readonly string[] _keysArray;
        public readonly int Width;
        public readonly int Height;

        // runtime
        public Camera Camera;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Color ClearColor;
        public List<Effect> PostEffectsList;

        // resources
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTargetA;
        private RenderTarget2D _renderTargetB;
        private RenderTarget2D _destinationRenderTarget;
        private Matrix _projection;

        public RenderPass(string key, Dictionary<string, Layer> layersDict, string[] keysArray, int? width, int? height)
        {
            // config
            Key = key;
            _layersDict = layersDict;
            _keysArray = keysArray;
            Width = width ?? WindowManager.NativeWidth;
            Height = height ?? WindowManager.NativeHeight;

            // runtime
            Camera = new Camera(_graphicsDevice.Viewport);
            BlendState = BlendState.AlphaBlend;
            SamplerState = SamplerState.PointClamp;
            ClearColor = Color.White;
            PostEffectsList = new List<Effect>();

            // resources
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _renderTargetA = new RenderTarget2D(_graphicsDevice, Width, Height);
            _renderTargetB = new RenderTarget2D(_graphicsDevice, Width, Height);
            _projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);;
        }

        public Layer GetLayer(string key)
        {
            if (!_layersDict.ContainsKey(key))
            {
                return null;
            }
            return _layersDict[key];
        }

        public void Render()
        {   
            _graphicsDevice.SetRenderTarget(_renderTargetA);

            _graphicsDevice.Clear(ClearColor);

            _graphicsDevice.BlendState = BlendState;
            _graphicsDevice.SamplerStates[0] = SamplerState;

            Matrix view = Camera.GetViewMatrix();
            Matrix transform = view * _projection;

            foreach (string key in _keysArray)
            {
                Layer layer = _layersDict[key];
                layer.Render(transform);
            }

            if (PostEffectsList.Count == 0)
            {
                return;
            }

            RenderTarget2D destinationRenderTarget = _renderTargetB;
            RenderTarget2D sourceRenderTarget = _renderTargetA;

            foreach (Effect effect in PostEffectsList)
            {
                _spriteBatch.Begin(effect: effect, blendState: BlendState, samplerState: SamplerState);
                _spriteBatch.Draw(sourceRenderTarget, destinationRenderTarget.Bounds, Color.White);
                _spriteBatch.End();

                (destinationRenderTarget, sourceRenderTarget) = (sourceRenderTarget, destinationRenderTarget);
            }

            _destinationRenderTarget = sourceRenderTarget;
        }

        public void Clear()
        {
            foreach (string key in _keysArray)
            {
                Layer layer = _layersDict[key];
                layer.Clear();
            }
        }
    }

    public sealed class Layer
    {
        private List<RenderObject> _renderObjectsList = new List<RenderObject>();

        public void Add(RenderObject renderObject)
        {
            _renderObjectsList.Add(renderObject);
        }

        public void Clear()
        {
            _renderObjectsList.Clear();
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
                Effect effect = renderSprite.Effect;

                if (texture != currentTexture || effect != currentEffect)
                {
                    Flush(batch, transform, effect);
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

    public static RenderPass GetRenderPass(string key)
    {
        if (!_renderPassesDict.ContainsKey(key))
        {
            return null;
        }
        return _renderPassesDict[key];
    }

    public static void FlushRenderTarget(RenderTarget2D renderTarget)
    {
        Sprite sprite = new Sprite(renderTarget);

        _vertices = new SpriteVertex[4];
        _indices = new short[6];
        ResizeBuffers();

        SpriteGeometry.CreateQuad(_vertices, 0, Vector2.Zero, sprite, Color.White, Degrees.Zero, (0, 0), Vector2.Zero, 1, 1, false, false);
        SpriteGeometry.CreateIndices(_indices, 0);
            
        _vertexBuffer.SetData(_vertices);
        _indexBuffer.SetData(_indices);

        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;

        _spriteEffect.Parameters["MatrixTransform"].SetValue(GetRenderTargetTransform(renderTarget) * GetScreenProjection());
        _spriteEffect.Parameters["SpriteTexture"].SetValue(sprite.Texture);

        foreach (EffectPass effectPass in _spriteEffect.CurrentTechnique.Passes)
        {
            int primitiveCount = 2;

            effectPass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
        }
    }

    private static Matrix GetRenderTargetTransform(RenderTarget2D renderTarget)
    {
        float scale = MathF.Min((float)_graphicsDevice.Viewport.Width / renderTarget.Width, (float)_graphicsDevice.Viewport.Height / renderTarget.Height);

        float x = (_graphicsDevice.Viewport.Width - renderTarget.Width * scale) * 0.5f;
        float y = (_graphicsDevice.Viewport.Height - renderTarget.Height * scale) * 0.5f;

        return Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(x, y, 0);
    }

    private static Matrix GetScreenProjection() => Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1);
    
}
*/