using Godot;

public class MaterialManager
{
    static MaterialManager _inst;

    public static MaterialManager inst
    {
        get
        {
            if (_inst == null)
                _inst = new MaterialManager();
            return _inst;
        }
    }

    ShaderMaterial _uberDefault;
    CanvasItemMaterial _addMaterial;

    /// <summary>
    /// 默认的 uber ShaderMaterial (ui_standard.gdshader)。
    /// 所有参数为默认值 (gray=0, blend=Normal)。
    /// 不需要特效的元素共享此实例以实现合批。
    /// </summary>
    public ShaderMaterial GetUberMaterial()
    {
        if (_uberDefault == null)
        {
            var shader = ResourceLoader.Load<Shader>("res://fgui/Resources/ui_standard.gdshader");
            _uberDefault = new ShaderMaterial();
            _uberDefault.Shader = shader;
        }
        return _uberDefault;
    }

    /// <summary>
    /// 克隆一个独立的 uber ShaderMaterial。
    /// 用于需要自定义参数（灰度、特殊混合模式等）的元素。
    /// </summary>
    public ShaderMaterial CloneUberMaterial()
    {
        return (ShaderMaterial)GetUberMaterial().Duplicate();
    }

    /// <summary>
    /// Add 混合模式使用 CanvasItemMaterial 的固定管线。
    /// </summary>
    public CanvasItemMaterial GetAddMaterial()
    {
        if (_addMaterial == null)
        {
            _addMaterial = new CanvasItemMaterial();
            _addMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
        }
        return _addMaterial;
    }
}
