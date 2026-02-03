using Godot;
using System.Collections.Generic;

namespace FairyGUI
{
    public enum MaterialType
    {
        StandardMaterial,
    }

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

        Dictionary<string, Material> _matDic = new Dictionary<string, Material>();
        private Shader _standardShader;

        public MaterialManager()
        {
            _standardShader = GD.Load<Shader>("res://fgui/Resources/ui_standard.gdshader");
        }

        public Material GetStandardMaterial(CanvasItemMaterial.BlendModeEnum blendMode)
        {
            string key = $"{blendMode}";
            Material mat;
            if (_matDic.TryGetValue(key, out mat))
            {
                return mat;
            }

            // 如果 Shader 加载失败，回退到原生的 CanvasItemMaterial
            if (_standardShader == null)
            {
                CanvasItemMaterial fallbackMat = new CanvasItemMaterial();
                fallbackMat.BlendMode = blendMode;
                _matDic.Add(key, fallbackMat);
                return fallbackMat;
            }

            ShaderMaterial newMat = new ShaderMaterial();
            newMat.Shader = _standardShader;
            // 处理混合模式映射（如果需要完全 1:1 还原 FGUI 各种 BlendMode，
            // 还需要在 Shader 中增加对应逻辑，目前主要针对核心合批优化）
            _matDic.Add(key, newMat);
            return newMat;
        }
    }
}
