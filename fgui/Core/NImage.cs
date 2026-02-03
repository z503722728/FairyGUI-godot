using FairyGUI.Utils;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NImage : Node2D, IDisplayObject
    {
        public enum ReverseType
        {
            None,
            All,
            OnlyColor,
        }
        protected Vector2 _position = Vector2.Zero;
        protected Vector2 _size = Vector2.Zero;
        protected Vector2 _pivot = Vector2.Zero;
        protected Vector2 _scale = Vector2.One;
        protected float _rotation = 0;
        protected Vector2 _skew = Vector2.Zero;
        protected Rect? _scale9Grid;
        protected bool _scaleByTile;
        protected Vector2 _textureScale = Vector2.One;
        protected int _tileGridIndice = 0;
        protected FlipType _flip = FlipType.None;
        protected FillMethod _fillMethod = FillMethod.None;
        protected int _fillOrigin = 0;
        protected float _fillAmount = 1f;
        protected bool _fillClockwise = true;
        protected NTexture _texture;
        protected Texture2D _reverseTexture;
        protected Material _material; // 统一为 Material 类型
        protected ArrayMesh _mesh;
        protected ArrayMesh _outBoundMesh;
        protected SurfaceTool _surfaceTool;
        internal IDisplayObject maskOwner;
        internal bool reverseMask = false;

        static Color outColor = Colors.White;

        public GObject gOwner { get; set; }
        public IDisplayObject parent { get { return GetParent() as IDisplayObject; } }
        public CanvasItem node { get { return this; } }
        public bool visible { get { return Visible; } set { Visible = value; } }
        public Vector2 skew
        {
            get { return _skew; }
            set
            {
                if (!_skew.IsEqualApprox(value))
                {
                    _skew = value;
                    UpdateTransform();
                }
            }
        }
        public float skewX
        {
            get { return _skew.X; }
            set
            {
                if (!Mathf.IsEqualApprox(_skew.X, value))
                {
                    _skew.X = value;
                    UpdateTransform();
                }
            }
        }
        public float skewY
        {
            get { return _skew.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(_skew.Y, value))
                {
                    _skew.Y = value;
                    UpdateTransform();
                }
            }
        }

        public void UpdateGrayed()
        {
            // V3架构：所有材质默认支持变灰，仅通过实例参数切换，永远不打断合批
            bool isGray = gOwner != null && gOwner.grayed;
            float val = isGray ? 1.0f : 0.0f;
            RenderingServer.CanvasItemSetInstanceShaderParameter(GetCanvasItem(), "gray_amount", val);
        }

        public Vector2 position
        {
            get { return Position; }
            set { SetPosition(position); }
        }
        public float X
        {
            get { return Position.X; }
            set { SetXY(value, Position.Y); }
        }
        public float Y
        {
            get { return Position.Y; }
            set { SetXY(Position.X, value); }
        }
        public void SetXY(float x, float y)
        {
            if (!Mathf.IsEqualApprox(_position.X, x) || !Mathf.IsEqualApprox(_position.Y, y))
            {
                _position.X = x;
                _position.Y = y;
                UpdatePosition();
                if (maskOwner != null)
                    QueueRedraw();
            }
        }
        public new void SetPosition(Vector2 pos)
        {
            if (!_position.IsEqualApprox(pos))
            {
                _position = pos;
                UpdatePosition();
                if (maskOwner != null)
                    QueueRedraw();
            }
        }
        public Vector2 size
        {
            get { return _size; }
            set { SetSize(value); }
        }
        public float width
        {
            get { return _size.X; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _size.X))
                {
                    _size.X = value;
                    UpdateTransform();
                }
            }
        }
        public float height
        {
            get { return _size.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _size.Y))
                {
                    _size.Y = value;
                    UpdateTransform();
                }
            }
        }
        public void SetSize(float w, float h)
        {
            if (!Mathf.IsEqualApprox(w, _size.X) || !Mathf.IsEqualApprox(h, _size.Y))
            {
                _size.X = w;
                _size.Y = h;
                UpdateTransform();
            }
        }
        public void SetSize(Vector2 size)
        {
            SetSize(size.X, size.Y);
        }
        public Vector2 pivot
        {
            get { return _pivot; }
            set
            {
                if (!_pivot.IsEqualApprox(value))
                {
                    _pivot = value;
                    UpdateTransform();
                }
            }
        }
        public float pivotX
        {
            get { return _pivot.X; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _pivot.X))
                {
                    _pivot.X = value;
                    UpdateTransform();
                }
            }
        }
        public float pivotY
        {
            get { return _pivot.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _pivot.Y))
                {
                    _pivot.Y = value;
                    UpdateTransform();
                }
            }
        }
        public Vector2 scale
        {
            get { return _scale; }
            set
            {
                if (!_scale.IsEqualApprox(value))
                {
                    _scale = value;
                    UpdateTransform();
                }
            }
        }
        public float scaleX
        {
            get { return _scale.X; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _scale.X))
                {
                    _scale.X = value;
                    UpdateTransform();
                }
            }
        }
        public float scaleY
        {
            get { return _scale.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _scale.Y))
                {
                    _scale.Y = value;
                    UpdateTransform();
                }
            }
        }
        public float rotation
        {
            get { return _rotation; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _rotation))
                {
                    _rotation = value;
                    UpdateTransform();
                }
            }
        }
        public BlendMode blendMode
        {
            get
            {
                if (_material != null && _material is ShaderMaterial shaderMat)
                {
                    // V3中我们目前统一使用 ShaderMaterial
                    return BlendMode.Normal; 
                }
                return BlendMode.Normal;
            }
            set
            {
                // V3架构：所有标准材质请求统统返回我们支持 instance 变灰的 ShaderMaterial
                CanvasItemMaterial.BlendModeEnum blendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                switch (value)
                {
                    case BlendMode.Normal:
                        blendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                        break;
                    case BlendMode.Add:
                        blendMode = CanvasItemMaterial.BlendModeEnum.Add;
                        break;
                    case BlendMode.Multiply:
                        blendMode = CanvasItemMaterial.BlendModeEnum.Mul;
                        break;
                    default:
                        blendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha;
                        break;
                }
                
                // 获取全局统一的、支持合批变灰的材质
                var newMat = MaterialManager.inst.GetStandardMaterial(blendMode);
                if (_material != newMat)
                {
                    _material = newMat;
                    Material = _material;
                }
            }
        }
        public NImage(GObject owner)
        {
            gOwner = owner;
            Init();
        }
        public NImage(NTexture texture)
            : base()
        {
            Init();
            if (texture != null)
                UpdateTexture(texture);
        }

        void Init()
        {
            Name = "Image";
            _mesh = new ArrayMesh();
            _surfaceTool = new SurfaceTool();
            // 默认初始化材质
            blendMode = BlendMode.Normal;
        }
        public NTexture texture
        {
            get { return _texture; }
            set
            {
                UpdateTexture(value);
            }
        }
        public Texture2D drawTexture
        {
            get
            {
                if (reverseMask)
                    return _reverseTexture;
                else
                    return _texture?.nativeTexture;
            }
        }
        public ArrayMesh mesh
        {
            get { return _mesh; }
        }
        public ArrayMesh outBoundMesh
        {
            get { return _outBoundMesh; }
        }
        public Vector2 textureScale
        {
            get { return _textureScale; }
            set
            {
                if (!Mathf.IsEqualApprox(_textureScale.X, value.X) || !Mathf.IsEqualApprox(_textureScale.Y, value.Y))
                {
                    _textureScale = value;
                    QueueRedraw();
                }
            }
        }
        public Color color
        {
            get
            {
                return Modulate;
            }
            set
            {
                Modulate = value;
            }
        }
        public void SetSize(float w, float h, bool ignorePivot)
        {
            SetSize(w, h);
        }
        public void UpdateTransform()
        {
            Vector2 actualPivot = _pivot;
            Position = _position;
            RotationDegrees = _rotation;
            Scale = _scale;
            QueueRedraw();
        }
        void UpdatePosition()
        {
            Position = _position;
        }

        public void QueueRedraw()
        {
            UpdateMesh();
        }

        public void SetNativeSize()
        {
            if (_texture != null)
                SetSize(_texture.width, _texture.height);
            else
                SetSize(0, 0);
        }

        void UpdateTexture(NTexture value)
        {
            if (value == _texture)
                return;
            _texture = value;
            _textureScale = Vector2.One;
            if (Mathf.IsEqualApprox(_size.X, 0))
                SetNativeSize();
            QueueRedraw();
        }

        public void UpdateMesh()
        {
            _mesh.ClearSurfaces();
            if (_texture == null)
            {
                return;
            }
            _surfaceTool.Clear();
            _surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            bool reserveDraw = maskOwner != null && reverseMask;
            int vertexCount = 0;

            Rect vertRect = new Rect(Vector2.Zero, _size);
            Rect uvRect = _texture.uvRect;
            TextureRepeat = TextureRepeatEnum.Disabled;

            if (reserveDraw && _texture != null)
            {
                Rect textRect = _texture.region;
                _reverseTexture = ToolSet.ExtractAndInvertAlpha(_texture.nativeTexture, new Rect2I((int)textRect.xMin, (int)textRect.yMin, (int)textRect.width, (int)textRect.height));
                uvRect.xMin = 0;
                uvRect.yMin = 0;
                uvRect.width = 1;
                uvRect.height = 1;
            }
            else
            {
                vertRect = _texture.GetDrawRect(vertRect, _flip);
            }

            if (_flip != FlipType.None)
            {
                if (_flip == FlipType.Horizontal || _flip == FlipType.Both)
                {
                    float tmp = uvRect.xMin;
                    uvRect.xMin = uvRect.xMax;
                    uvRect.xMax = tmp;
                }
                if (_flip == FlipType.Vertical || _flip == FlipType.Both)
                {
                    float tmp = uvRect.yMin;
                    uvRect.yMin = uvRect.yMax;
                    uvRect.yMax = tmp;
                }
            }
            TextureRepeat = TextureRepeatEnum.Disabled;
            if (_fillMethod != FillMethod.None)
            {
                switch (_fillMethod)
                {
                    case FillMethod.Horizontal:
                        vertexCount += FillHorizontal(_surfaceTool, vertRect, uvRect, _fillOrigin, _fillAmount, vertexCount);
                        break;

                    case FillMethod.Vertical:
                        vertexCount += FillVertical(_surfaceTool, vertRect, uvRect, _fillOrigin, _fillAmount, vertexCount);
                        break;

                    case FillMethod.Radial90:
                        vertexCount += FillRadial90(_surfaceTool, vertRect, uvRect, (Origin90)_fillOrigin, _fillAmount, _fillClockwise, vertexCount);
                        break;

                    case FillMethod.Radial180:
                        vertexCount += FillRadial180(_surfaceTool, vertRect, uvRect, (Origin180)_fillOrigin, _fillAmount, _fillClockwise, vertexCount);
                        break;

                    case FillMethod.Radial360:
                        vertexCount += FillRadial360(_surfaceTool, vertRect, uvRect, (Origin360)_fillOrigin, _fillAmount, _fillClockwise, vertexCount);
                        break;
                }
            }
            else if (_scaleByTile)
            {
                if (_texture.root == _texture && _texture.nativeTexture != null)
                {
                    //独立纹理，可以直接使用tile模式
                    TextureRepeat = TextureRepeatEnum.Enabled;
                    uvRect.width *= vertRect.width / texture.width * _textureScale.X;
                    uvRect.height *= vertRect.height / texture.height * _textureScale.Y;
                    ToolSet.MeshAddRect(_surfaceTool, vertRect, uvRect, 0);
                    vertexCount += 4;
                }
                else
                {
                    Rect contentRect = vertRect;
                    contentRect.width *= _textureScale.X;
                    contentRect.height *= _textureScale.Y;

                    vertexCount += TileFill(_surfaceTool, contentRect, uvRect, texture.width, texture.height, 0);
                }
            }
            else if (_scale9Grid != null)
            {
                vertexCount += SliceFill(_surfaceTool, vertRect, uvRect, texture.width, texture.height);
            }
            else
            {
                ToolSet.MeshAddRect(_surfaceTool, vertRect, uvRect, 0);
                vertexCount += 4;
            }

            // _surfaceTool.GenerateNormals();
            if (_material != null)
                _surfaceTool.SetMaterial(_material);
            _surfaceTool.Commit(_mesh);

            if (reserveDraw)
            {
                if (_outBoundMesh == null)
                    _outBoundMesh = new ArrayMesh();
                else
                    _outBoundMesh.ClearSurfaces();
                _surfaceTool.Clear();
                _surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
                vertexCount = 0;
                if (_fillMethod != FillMethod.None)
                {
                    switch (_fillMethod)
                    {
                        case FillMethod.Horizontal:
                            vertexCount += FillHorizontal(_surfaceTool, vertRect, uvRect, _fillOrigin, _fillAmount, vertexCount, ReverseType.All);
                            break;

                        case FillMethod.Vertical:
                            vertexCount += FillVertical(_surfaceTool, vertRect, uvRect, _fillOrigin, _fillAmount, vertexCount, ReverseType.All);
                            break;

                        case FillMethod.Radial90:
                            vertexCount += FillRadial90(_surfaceTool, vertRect, uvRect, (Origin90)_fillOrigin, _fillAmount, _fillClockwise, vertexCount, ReverseType.All);
                            break;

                        case FillMethod.Radial180:
                            vertexCount += FillRadial180(_surfaceTool, vertRect, uvRect, (Origin180)_fillOrigin, _fillAmount, _fillClockwise, vertexCount, ReverseType.All);
                            break;

                        case FillMethod.Radial360:
                            vertexCount += FillRadial360(_surfaceTool, vertRect, uvRect, (Origin360)_fillOrigin, _fillAmount, _fillClockwise, vertexCount, ReverseType.All);
                            break;
                    }
                }
                DrawOutBound(vertexCount);
                // _surfaceTool.GenerateNormals();
                _surfaceTool.Commit(_outBoundMesh);
            }
        }

        Rect MakeOutRect(float offsetX = 0, float offsetY = 0)
        {
            if (maskOwner == null)
                return new Rect(-offsetX, -offsetY, _size.X + offsetX * 2, _size.Y + offsetY * 2);
            Transform2D Trans = GetTransform();
            Vector2 min = Trans * new Vector2(-offsetX, -offsetY);
            Vector2 max = Trans * (_size + new Vector2(offsetX, offsetY));
            min.X = Mathf.Min(min.X, 0);
            min.Y = Mathf.Min(min.Y, 0);
            max.X = Mathf.Max(max.X, maskOwner.size.X);
            max.Y = Mathf.Max(max.Y, maskOwner.size.Y);
            Trans = GetTransform().AffineInverse();
            min = Trans * min;
            max = Trans * max;
            return Rect.MinMaxRect(
                Mathf.Min(min.X, max.X), Mathf.Min(min.Y, max.Y),
                Mathf.Max(min.X, max.X), Mathf.Max(min.Y, max.Y));
        }
        void DrawOutBound(int vertexStart)
        {
            Rect rect = new Rect(Vector2.Zero, _size);
            Rect outRect = MakeOutRect();
            ToolSet.MeshAddVertex(_surfaceTool, rect.xMin, rect.yMin, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, rect.xMax, rect.yMin, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, rect.xMax, rect.yMax, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, rect.xMin, rect.yMax, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, outRect.xMin, outRect.yMin, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, outRect.xMax, outRect.yMin, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, outRect.xMax, outRect.yMax, outColor);
            ToolSet.MeshAddVertex(_surfaceTool, outRect.xMin, outRect.yMax, outColor);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart, vertexStart + 4, vertexStart + 5);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart, vertexStart + 5, vertexStart + 1);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 1, vertexStart + 5, vertexStart + 6);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 1, vertexStart + 6, vertexStart + 2);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 2, vertexStart + 6, vertexStart + 7);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 2, vertexStart + 7, vertexStart + 3);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 3, vertexStart + 7, vertexStart + 4);
            ToolSet.MeshAddTriangleIndecies(_surfaceTool, vertexStart + 3, vertexStart + 4, vertexStart + 0);
        }

        int FillHorizontal(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, int origin, float amount, int startIndex, ReverseType reverse = ReverseType.None)
        {
            if (reverse == ReverseType.All)
            {
                if ((OriginHorizontal)origin == OriginHorizontal.Right)
                    origin = (int)OriginHorizontal.Left;
                else
                    origin = (int)OriginHorizontal.Right;
            }

            float x;
            if ((OriginHorizontal)origin == OriginHorizontal.Left)
                x = vertRect.X + vertRect.width * amount;
            else
                x = vertRect.X + vertRect.width * (1 - amount);

            float u = Mathf.Lerp(uvRect.X, uvRect.xMax, (x - vertRect.X) / vertRect.width);

            if ((OriginHorizontal)origin == OriginHorizontal.Left)
            {
                ToolSet.MeshAddRect(surfaceTool, new Rect(vertRect.X, vertRect.Y, x - vertRect.X, vertRect.height),
                    new Rect(uvRect.X, uvRect.Y, u - uvRect.X, uvRect.height), startIndex);
            }
            else
            {
                ToolSet.MeshAddRect(surfaceTool, new Rect(x, vertRect.Y, vertRect.xMax - x, vertRect.height),
                    new Rect(u, uvRect.Y, uvRect.xMax - u, uvRect.height), startIndex);
            }

            return 4;
        }

        int FillVertical(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, int origin, float amount, int startIndex, ReverseType reverse = ReverseType.None)
        {
            if (reverse == ReverseType.All)
            {
                if ((OriginVertical)origin == OriginVertical.Bottom)
                    origin = (int)OriginVertical.Top;
                else
                    origin = (int)OriginVertical.Bottom;
            }

            float y;
            if ((OriginVertical)origin == OriginVertical.Top)
                y = vertRect.Y + vertRect.height * amount;
            else
                y = vertRect.Y + vertRect.height * (1 - amount);

            float v = Mathf.Lerp(uvRect.Y, uvRect.yMax, (y - vertRect.Y) / vertRect.height);

            if ((OriginVertical)origin == OriginVertical.Top)
            {
                ToolSet.MeshAddRect(surfaceTool, new Rect(vertRect.X, vertRect.Y, vertRect.width, y - vertRect.Y),
                    new Rect(uvRect.X, uvRect.Y, uvRect.width, v - uvRect.Y), startIndex);
            }
            else
            {
                ToolSet.MeshAddRect(surfaceTool, new Rect(vertRect.X, y, vertRect.width, vertRect.yMax - y),
                    new Rect(uvRect.X, v, uvRect.width, uvRect.yMax - v), startIndex);
            }

            return 4;
        }

        int FillRadial90(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, Origin90 origin, float amount, bool clockwise, int startIndex, ReverseType reverse = ReverseType.None)
        {
            if (reverse == ReverseType.All)
                clockwise = !clockwise;

            return ToolSet.MeshAddRadial90(surfaceTool, vertRect, uvRect, origin, amount, clockwise, startIndex);
        }

        int FillRadial180(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, Origin180 origin, float amount, bool clockwise, int startIndex, ReverseType reverse = ReverseType.None)
        {
            if (reverse == ReverseType.All)
                clockwise = !clockwise;

            return ToolSet.MeshAddRadial180(surfaceTool, vertRect, uvRect, origin, amount, clockwise, startIndex);
        }

        int FillRadial360(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, Origin360 origin, float amount, bool clockwise, int startIndex, ReverseType reverse = ReverseType.None)
        {
            if (reverse == ReverseType.All)
                clockwise = !clockwise;

            return ToolSet.MeshAddRadial360(surfaceTool, vertRect, uvRect, origin, amount, clockwise, startIndex);
        }

        int SliceFill(SurfaceTool surfaceTool, Rect vertRect, Rect uvRect, float sourceW, float sourceH)
        {
            Rect gridRect = _scale9Grid.Value;
            Rect contentRect = vertRect;
            float xMax2 = sourceW - gridRect.xMax;
            float yMax2 = sourceH - gridRect.yMax;

            float[] gridX = new float[4];
            float[] gridY = new float[4];
            float[] gridTexX = new float[4];
            float[] gridTexY = new float[4];

            gridTexX[0] = uvRect.X;
            gridTexX[1] = Mathf.Lerp(uvRect.X, uvRect.xMax, gridRect.X / sourceW);
            gridTexX[2] = Mathf.Lerp(uvRect.X, uvRect.xMax, gridRect.xMax / sourceW);
            gridTexX[3] = uvRect.xMax;

            gridTexY[0] = uvRect.Y;
            gridTexY[1] = Mathf.Lerp(uvRect.Y, uvRect.yMax, gridRect.Y / sourceH);
            gridTexY[2] = Mathf.Lerp(uvRect.Y, uvRect.yMax, gridRect.yMax / sourceH);
            gridTexY[3] = uvRect.yMax;

            if (contentRect.width >= (sourceW - gridRect.width))
            {
                gridX[1] = gridRect.X;
                gridX[2] = contentRect.width - xMax2;
                gridX[3] = contentRect.width;
            }
            else
            {
                float tmp = gridRect.X / (sourceW - xMax2);
                tmp = contentRect.width * tmp / (1 + tmp);
                gridX[1] = tmp;
                gridX[2] = tmp;
                gridX[3] = contentRect.width;
            }

            if (contentRect.height >= (sourceH - gridRect.height))
            {
                gridY[1] = gridRect.Y;
                gridY[2] = contentRect.height - (sourceH - yMax2);
                gridY[3] = contentRect.height;
            }
            else
            {
                float tmp = gridRect.Y / (sourceH - yMax2);
                tmp = contentRect.height * tmp / (1 + tmp);
                gridY[1] = tmp;
                gridY[2] = tmp;
                gridY[3] = contentRect.height;
            }

            if (_tileGridIndice == 0)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    for (int cx = 0; cx < 4; cx++)
                    {
                        surfaceTool.SetUV(new Vector2(gridTexX[cx], gridTexY[cy]));
                        surfaceTool.AddVertex(new Vector3(gridX[cx] / _textureScale.X, gridY[cy] / _textureScale.Y, 0));
                    }
                }
                for (int i = 0; i < TRIANGLES_9_GRID.Length; i++)
                {
                    surfaceTool.AddIndex(TRIANGLES_9_GRID[i]);
                }
            }
            else
            {
                Rect drawRect;
                Rect texRect;
                int row, col;
                int part;

                for (int pi = 0; pi < 9; pi++)
                {
                    col = pi % 3;
                    row = pi / 3;
                    part = gridTileIndice[pi];
                    drawRect = Rect.MinMaxRect(gridX[col], gridY[row], gridX[col + 1], gridY[row + 1]);
                    texRect = Rect.MinMaxRect(gridTexX[col], gridTexY[row], gridTexX[col + 1], gridTexY[row + 1]);

                    if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
                    {
                        TileFill(surfaceTool, drawRect, texRect, gridRect.width, gridRect.height, 0);
                    }
                    else
                    {
                        ToolSet.MeshAddRect(surfaceTool, drawRect, texRect, 0);
                    }
                }
            }

            return 16;
        }

        int TileFill(SurfaceTool surfaceTool, Rect contentRect, Rect uvRect, float sourceW, float sourceH, int StartIndex)
        {
            int hc = Mathf.CeilToInt(contentRect.width / sourceW);
            int vc = Mathf.CeilToInt(contentRect.height / sourceH);
            float tailWidth = contentRect.width - (hc - 1) * sourceW;
            float tailHeight = contentRect.height - (vc - 1) * sourceH;
            float xMax = uvRect.xMax;
            float yMax = uvRect.yMax;
            int firstIndex = StartIndex;

            for (int i = 0; i < hc; i++)
            {
                for (int j = 0; j < vc; j++)
                {
                    Rect uvTmp = uvRect;
                    if (i == hc - 1)
                        uvTmp.xMax = Mathf.Lerp(uvRect.X, xMax, tailWidth / sourceW);
                    if (j == vc - 1)
                        uvTmp.yMax = Mathf.Lerp(uvRect.Y, yMax, tailHeight / sourceH);

                    Rect drawRect = new Rect(contentRect.X + i * sourceW, contentRect.Y + j * sourceH,
                            i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH);

                    drawRect.X /= _textureScale.X;
                    drawRect.Y /= _textureScale.Y;
                    drawRect.width /= _textureScale.X;
                    drawRect.height /= _textureScale.Y;

                    ToolSet.MeshAddRect(surfaceTool, drawRect, uvTmp, StartIndex);
                    StartIndex += 4;
                }
            }
            return StartIndex - firstIndex;
        }

        public override void _Draw()
        {
            UpdateMesh();
            if (maskOwner != null)
            {
                maskOwner.node.QueueRedraw();
                return;
            }            
            DrawMesh(_mesh, _texture?.nativeTexture);
        }

        static readonly int[] TRIANGLES_9_GRID = new int[] {
            4, 0, 1, 1, 5, 4,
            5, 1, 2, 2, 6, 5,
            6, 2, 3, 3, 7, 6,
            8, 4, 5, 5, 9, 8,
            9, 5, 6, 6, 10, 9,
            10, 6, 7, 7, 11, 10,
            12, 8, 9, 9, 13, 12,
            13, 9, 10, 10, 14, 13,
            14, 10, 11, 11, 15, 14
        };

        static readonly int[] gridTileIndice = new int[] { -1, 0, -1, 1, 4, 2, -1, 3, -1 };

        public void SetFlip(FlipType value)
        {
            if (_flip != value)
            {
                _flip = value;
                QueueRedraw();
            }
        }
    }
}
