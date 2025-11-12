using Godot;
using System;

namespace FairyGUI
{
    public interface IDisplayObject
    {
        GObject gOwner { get; set; }
        IDisplayObject parent { get; }
        Control node { get; }
        bool visible { get; set; }
        float skewX { get; set; }
        float skewY { get; set; }
        float X { get; set; }
        float Y { get; set; }
        void SetXY(float x, float y);
        void SetPosition(Vector2 pos);
        public float width { get; set; }
        public float height { get; set; }
        void SetSize(float w, float h);
        void SetSize(Vector2 size);
        BlendMode blendMode { get; set; }
        event System.Action<double> onUpdate;
    }
}