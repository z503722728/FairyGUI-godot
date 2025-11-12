using FairyGUI;
using Godot;
using System;

namespace FairyGUI
{
	public partial class NContainer3D : SubViewportContainer, IDisplayObject
	{
		// Called when the node enters the scene tree for the first time.
		public GObject gOwner { get; set; }
		public IDisplayObject parent { get { return GetParent() as IDisplayObject; } }
		public Control node { get { return this; } }
		public bool visible { get { return Visible; } set { Visible = value; } }
		public float skewX { get; set; }
		public float skewY { get; set; }
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
            Position = new Vector2(x, y);
        }
        public void SetPosition(Vector2 pos)
        {
            Position = pos;
        }
        public float width
        {
            get { return Size.X; }
            set
            {
                SetSize(value, Size.Y);
            }
        }
        public float height
        {
            get { return Size.Y; }
            set
            {
                SetSize(Size.X, value);
            }
        }
        public void SetSize(float w, float h)
        {
            if (!Mathf.IsEqualApprox(w, Size.X) || !Mathf.IsEqualApprox(h, Size.Y))
                Size = new Vector2(w, h);
        }
        public void SetSize(Vector2 size)
        {
            if (!Size.IsEqualApprox(size))
                Size = size;
        }
		public BlendMode blendMode { get; set; }
		public event System.Action<double> onUpdate;
		public NContainer3D(GObject owner)
		{
			gOwner = owner;
			MouseFilter = MouseFilterEnum.Ignore;
		}
		public override void _Process(double delta)
		{
			if (onUpdate != null)
				onUpdate(delta);
		}		
	}
}