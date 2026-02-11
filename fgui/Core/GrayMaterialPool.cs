using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 全局共享的变灰 ShaderMaterial 单例池。
    /// 所有需要变灰的 CanvasItem 共用同一实例，避免重复创建。
    /// </summary>
    public static class GrayMaterialPool
    {
        static ShaderMaterial _mat;

        public static ShaderMaterial Get()
        {
            if (_mat == null)
            {
                var shader = GD.Load<Shader>("res://fgui/Resources/ui_grayscale.gdshader");
                if (shader != null)
                {
                    _mat = new ShaderMaterial();
                    _mat.Shader = shader;
                }
            }
            return _mat;
        }
    }
}
