using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine.Gui
{
    public class GUIPanel
    {
        public GUIPanel? parent;
        private List<GUIPanel> children;
        public Vector2 anchor;
        public Vector2 position;
        public Vector2 size;
        public bool visible;
        public string name;
        public Color tint;

        public GUIPanel(string name, Vector2 pos, Vector2 size, Vector2 anchor, Color tint)
        {
            this.tint = tint;
            this.name = name;
            this.position = pos;
            this.size = size;
            this.anchor = anchor;
            this.visible = true;
            children = new List<GUIPanel>();
        }

        public GUIPanel()
        {
            this.tint = Color.White;
            this.name = "Untitled";
            this.position = Vector2.Zero;
            this.size = Vector2.One*16;
            this.anchor = Vector2.Zero;
            this.visible = true;
            children = new List<GUIPanel>();
        }

        public GUIPanel(string name)
        {
            this.tint = Color.White;
            this.name = name;
            this.position = Vector2.Zero;
            this.size = Vector2.One * 16;
            this.anchor = Vector2.Zero;
            this.visible = true;
            children = new List<GUIPanel>();
        }

        public void AddChild(GUIPanel child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void RemoveChild(GUIPanel child)
        {
            children.Remove(child);
        }

        public GUIPanel? FindChildByName(string name)
        {
            for (int i = 0; i<children.Count; i++)
            {
                var child = children[i];
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        public bool IsMouseOver()
        {
            if (!visible) { return false; }

            int x = Raylib.GetMouseX();
            int y = Raylib.GetMouseY();
            Vector2 drawpos = GetDrawPos();

            //Console.WriteLine(x);

            return x > drawpos.X && x < drawpos.X + size.X && y > drawpos.Y && y < drawpos.Y + size.Y;
        }

        private bool IsMouseOverParent()
        {
            if (parent == null) { return false; }

            if (parent.IsMouseOver()) { return true; }

            return parent.IsMouseOverParent();
        }

        public bool GetMouseOver()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                bool over = child.IsMouseOver();
                if (over)
                {
                    return false;
                }
            }
            return IsMouseOver();
        }

        public virtual void Update()
        {
            if (!visible) { return; }
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.Update();
            }
        }

        public Vector2 GetDrawPos()
        {
            Vector2 drawpos = position;
            if (parent != null)
            {
                drawpos += parent.GetDrawPos() + (parent.size * parent.anchor);
            }
            drawpos -= size * anchor;
            return drawpos;
        }

        public virtual void DrawSelf()
        {
            Vector2 drawpos = GetDrawPos();
            Raylib.DrawRectangle((int)drawpos.X, (int)drawpos.Y, (int)size.X, (int)size.Y, tint);
        }

        public void DrawChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.Draw();
            }
        }

        public virtual void Draw()
        {
            if (!visible) { return; }

            DrawSelf();

            DrawChildren();
        }
    }
}
