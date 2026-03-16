using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
using blockengine.Items;

namespace blockengine.Gui
{
    public class InventoryPanel : GUIPanel
    {
        private Inventory inventory;
        private List<Vector2> slot_positions;
        private int slotscale = 64;
        private float slotpadding = 0.04f;
        public int selected_slot = -1;
        private int start_drag_pos = -1;
        public bool interactable = true;

        private int offset = 0;

        public InventoryPanel(Inventory inv,int _offset = 0)
        {
            offset = _offset;
            inventory = inv;
            
            slot_positions = new List<Vector2>();
            this.tint = new Color(0.5f, 0.5f, 1f, 0.8f);
        }

        public void SetLayoutNormal(bool hotbar = false)
        {
            
            
            int slotcount = inventory.GetSlotCount() - offset;
            if (hotbar)
            {
                this.size = new Vector2(9 * slotscale, ((slotcount / 9) + 0.25f) * slotscale);
            }
            else
            {
                this.size = new Vector2(9 * slotscale, (slotcount / 9) * slotscale);
            }

            slot_positions.Clear();
            for (int i = 0; i < slotcount; i++)
            {
                if (i >= slotcount-9 && hotbar)
                {
                    slot_positions.Add(
                        new Vector2(
                            (i % 9),
                            (i / 9) + 0.25f
                        )
                    );
                }
                else
                {
                    slot_positions.Add(
                        new Vector2(
                            (i % 9),
                            (i / 9)
                        )
                    );
                }
            }
        }

        private int GetHoveredSlot()
        {
            for (int i = 0; i < slot_positions.Count; i++)
            {
                Vector2 slotpos = slot_positions[i];
                Vector2 drawpos = GetDrawPos();
                float drawx = drawpos.X + (slotpos.X * slotscale);
                float drawy = drawpos.Y + (slotpos.Y * slotscale);

                int mx = Raylib.GetMouseX();
                int my = Raylib.GetMouseY();

                if (mx > drawx && mx < drawx + slotscale && my > drawy && my < drawy + slotscale)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void Update()
        {
            if (!visible || !interactable) { return; }

            selected_slot = -1;
            if (IsMouseOver())
            {
                selected_slot = GetHoveredSlot();

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    start_drag_pos = selected_slot;
                }

                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    inventory.SlotDragged(offset + start_drag_pos, offset + selected_slot);
                    start_drag_pos = -1;
                }
            }

            base.Update();
        }

        public override void DrawSelf()
        {
            Vector2 drawpos = GetDrawPos();
            Raylib.DrawRectangleGradientV((int)drawpos.X, (int)drawpos.Y, (int)size.X, (int)size.Y, tint,new Color(tint.R/3, tint.G/3, tint.B/3));
        }

        public override void Draw()
        {
            if (visible)
            {
                DrawSelf();

                int pad = slotscale/16;

                Vector2 drawpos = GetDrawPos();


                for (int i = 0; i < slot_positions.Count; i++)
                {
                    InventorySlot slot = inventory.GetSlot(offset+i);

                    Vector2 slotpos = slot_positions[i];

                    float drawx = drawpos.X + (slotpos.X * slotscale);
                    float drawy = drawpos.Y + (slotpos.Y * slotscale);

                    Raylib.DrawRectangle((int)drawx+ pad, (int)drawy+ pad, slotscale-(pad*2), slotscale-(pad*2), new Color(0f, 0f, 0f, 0.5f));

                    if (slot.count > 0)
                    {
                        Item itemdef = Globals.ItemDefinitions[slot.item];
                        Raylib.DrawTexturePro(TextureHandler.block_atlas.Texture, TextureHandler.GetTextureUV(itemdef.texture).ToRectangle(), new Rectangle(-slotscale/3,-slotscale/3,slotscale/1.5f,slotscale/1.5f), new Vector2(-(drawx + slotscale / 2), -(drawy + slotscale / 2)), 0, Color.White);

                        if (slot.count > 1)
                        {
                            Raylib.DrawText(slot.count.ToString(), (int)drawx + pad, (int)drawy + pad, 20, Color.White);
                        }
                    }

                    if (i == selected_slot || i == start_drag_pos)
                    {
                        Raylib.DrawRectangleLines((int)drawx + pad, (int)drawy + pad, slotscale - (pad * 2), slotscale - (pad * 2), Color.White);
                    }
                }

                DrawChildren();
            }
        }
    }
}
