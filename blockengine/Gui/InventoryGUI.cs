using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

namespace blockengine.Gui
{
    public class InventorySpotGUI : GUIPanel
    {
        //private Rectangle tex_rect;
        private Texture2D sprite;
        public InventorySpotGUI()
        {
            sprite = TextureHandler.GetSprite("Assets/Textures/Sprites/inventory_spot.png");
            anchor = Vector2.One * 0.5f;
            size = new Vector2(48f, 48f);
        }
        public override void Draw()
        {
            if (!visible) { return; }

            Vector2 drawpos = GetDrawPos();

            Raylib.DrawTextureEx(sprite,drawpos,0,2,tint);
            //Raylib.DrawText("hey", (int)drawpos.X, (int)drawpos.Y, 24, tint);
        }

        public override void Update()
        {
            if (!visible) { return; }
            if (IsMouseOver())
            {
                tint = new Color(200, 200, 200);
            }
            else
            {
                tint = Color.White;
            }
        }

    }
    public class InventoryGUI
    {
        private GUIPanel mainpanel;
        private GUIPanel inventorypanel;
        public bool inv_showing;

        public InventoryGUI()
        {
            mainpanel = new GUIPanel("hotbar",new Vector2(640,720- 64),new Vector2(32,32),new Vector2(0.5f,0.5f),Color.White);
            inventorypanel = new GUIPanel("inventory");
            inventorypanel.position.Y = -24f;
            mainpanel.AddChild(inventorypanel);

            var _w = 9f * 48f;
            for (int i = 0; i<9; i++)
            {
                var _x = (i * 48f) - _w / 2;
                var invspot = new InventorySpotGUI();
                invspot.position = new Vector2(_x, 0);

                mainpanel.AddChild(invspot);
            }

            var _h = 3f * 48f;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var _x = (i * 48f) - _w / 2;
                    var invspot = new InventorySpotGUI();
                    invspot.position = new Vector2(_x, -_h + (j * 48f));

                    inventorypanel.AddChild(invspot);
                }
            }

            inventorypanel.visible = false;
        }

        public void Draw()
        {
            mainpanel.Draw();
        }

        public void Update()
        {
            mainpanel.Update();
        }

        public void ToggleInventory()
        {
            inventorypanel.visible = !inventorypanel.visible;
            if (inventorypanel.visible)
            {
                Raylib.EnableCursor();
                inv_showing = true;
            }
            else
            {
                Raylib.DisableCursor();
                inv_showing = false;
            }
        }
    }
}
