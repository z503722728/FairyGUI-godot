using FairyGUI;
using Godot;
using System;

namespace FairyGUI
{
	public partial class NContainer : Control, IDisplayObject
	{
		// Called when the node enters the scene tree for the first time.
		protected Control _mask;
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
		public NContainer(GObject owner)
		{
			gOwner = owner;
			MouseFilter = MouseFilterEnum.Ignore;
			FocusEntered += () => { if (gOwner != null) gOwner.DispatchEvent("onFocusIn"); };
			FocusExited += () => { if (gOwner != null) gOwner.DispatchEvent("onFocusOut"); };
		}
		public override void _Process(double delta)
		{
			if (onUpdate != null)
				onUpdate(delta);
		}

		public Control mask
		{
			get { return _mask; }
			set
			{
				if (_mask != value)
				{
					if (value == null)
					{
						if (_mask is NImage image)
						{
							image.maskOwner = null;
							image.QueueRedraw();
						}
						else if (_mask is NShape shape)
						{
							shape.maskOwner = null;
							shape.QueueRedraw();
						}
					}
					_mask = value;
					if (_mask != null)
					{
						ClipChildren = ClipChildrenMode.Only;
						TextureRepeat = _mask.TextureRepeat;
						if (_mask is NImage image)
						{
							image.maskOwner = this;
							image.QueueRedraw();
						}
						else if (_mask is NShape shape)
						{
							shape.maskOwner = this;
							shape.QueueRedraw();
						}
					}
					else
					{
						ClipChildren = ClipChildrenMode.Disabled;
					}
					QueueRedraw();
				}
			}
		}
		public bool reversedMask
		{
			get
			{
				if (_mask is NImage image)
				{
					return image.reverseMask;
				}
				else if (_mask is NShape shape)
				{
					return shape.reverseMask;
				}
				return false;
			}
			set
			{
				if (_mask is NImage image)
				{
					image.reverseMask = value;
				}
				else if (_mask is NShape shape)
				{
					shape.reverseMask = value;
				}
			}
		}
		public override void _Draw()
		{
			if (_mask != null)
			{
				if (_mask is NImage image)
				{
					Transform2D trans = image.GetTransform();
					image.UpdateMesh();
					DrawMesh(image.mesh, image.drawTexture, trans);
					if (image.outBoundMesh != null)
						DrawMesh(image.outBoundMesh, null, trans);
				}
				else if (_mask is NShape shape)
				{
					Transform2D trans = shape.GetTransform();
					shape.UpdateMesh();
					DrawMesh(shape.mesh, shape.texture?.nativeTexture, trans);
				}
			}
		}
	}
}