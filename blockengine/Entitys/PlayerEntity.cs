using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using static Raylib_cs.Raymath;
using Raylib_cs;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using blockengine.Gui;
using blockengine.Items;

namespace blockengine.Entitys
{
    public class PlayerEntity : LivingEntity
    {
        public Camera3D cam;
        public Inventory inventory;

        private float camlookyaw = 0;
        private float camlookpitch = 0;
        private float last_camlookyaw = 0;
        private float last_camlookpitch = 0;
        private Vector2 camdelta;

        private float gravity = 0.25f;
        private float mousesensitivity = 30f;
        private float spd = 4f;
        private Mesh? handmesh = null;

        private bool focused_on_block = false;
        private BlockType focused_blocktype;
        private Int3 focused_block_pos;
        private Block? focused_blockdef = null;
        private RaycastResult? last_raycast = null;
        private float block_health = 0;
        private float max_block_health = 0;
        private BlockFaces<string>[] crack_anim;
        private float hit_anim = 0;
        private float bob_anim = 0;

        private GUIPanel GUIroot;
        private InventoryPanel invpanel;
        private InventoryPanel hotbarpanel;
        private bool mouse_locked = true;

        private int selected_hotbar = 0;
        private int selected_slot = -1;
        private bool holding_something = false;
        private ItemType holding_itemtype = 0;

        private bool noclipping = false;
        //private RenderTexture2D viewmodel_rendertexture;
        public PlayerEntity(World _world, string _Name, Vector3 _position) : base(_world, _Name, _position)
        {
            max_health = 100;
            health = max_health;

            crack_anim = new BlockFaces<string>[5] {
            new BlockFaces<string>("Assets/Textures/crack5.png"),
            new BlockFaces<string>("Assets/Textures/crack4.png"),
            new BlockFaces<string>("Assets/Textures/crack3.png"),
            new BlockFaces<string>("Assets/Textures/crack2.png"),
            new BlockFaces<string>("Assets/Textures/crack1.png")
            };

            var collider_width = 0.75f;
            var collider_height = 1.5f;
            SetCollider(Vector3.Zero, new Vector3(-collider_width / 2, -collider_width / 2, -collider_height / 2), new Vector3(collider_width / 2, collider_width / 2, collider_height / 2));

            inventory = new Inventory(9 * 4);
            GUIroot = new GUIPanel("root", new Vector2(0, 0), new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight()),new Vector2(0,0),new Color(0,0,0,0));
            invpanel = new InventoryPanel(inventory);
            invpanel.SetLayoutNormal(true);
            invpanel.position = new Vector2(Raylib.GetRenderWidth() / 2, Raylib.GetRenderHeight() / 2);
            invpanel.anchor = new Vector2(0.5f, 0.5f);
            invpanel.visible = false;

            hotbarpanel = new InventoryPanel(inventory, 9*3);
            hotbarpanel.SetLayoutNormal();
            hotbarpanel.position = new Vector2(Raylib.GetRenderWidth() / 2, Raylib.GetRenderHeight());
            hotbarpanel.anchor = new Vector2(0.5f, 1f);
            hotbarpanel.visible = true;
            hotbarpanel.interactable = false;

            GUIroot.AddChild(invpanel);
            GUIroot.AddChild(hotbarpanel);

            SelectHotbar(0);
        }

        public override void Start()
        {
            world.cam.Position = Position;

            Raylib.DisableCursor();
        }

        private void unstuck()
        {
            Position += new Vector3(Raylib.GetRandomValue(-1, 1), Raylib.GetRandomValue(-1, 1), Raylib.GetRandomValue(-1, 1));
        }

        private void SelectHotbar(int newselected)
        {
            int select = Globals.better_modI(newselected, 9);

            selected_hotbar = select;
            selected_slot = (inventory.GetSlotCount() - 9) + selected_hotbar;

            InventorySlot slot = inventory.GetSlot(selected_slot);
            holding_something = !slot.IsEmpty();
            holding_itemtype = slot.item;

            hotbarpanel.selected_slot = selected_hotbar;
        }

        private void setblocklookingat()
        {
            last_raycast = world.Raycast(world.cam.Position, GetCameraForward() * 8);
            if (last_raycast != null)
            {
                ChunkBlock? block = world.GetBlock(last_raycast.BlockPosition);
                if (block != null && last_raycast.BlockPosition != focused_block_pos)
                {
                    focused_on_block = true;

                    focused_block_pos = last_raycast.BlockPosition;
                    focused_blocktype = block.block;
                    focused_blockdef = block.GetBlockDef();

                    block_health = focused_blockdef.GetDurability();
                    max_block_health = block_health;
                }
            }
            else
            {
                focused_on_block = false;
            }
        }

        public override void Update(float deltatime)
        {
            UpdateCamera(deltatime);

            Vector3 camforward = Vector3.Normalize(GetCameraForward() * new Vector3(1, 1, 0));
            Vector3 camright = Vector3.Normalize(GetCameraRight() * new Vector3(1, 1, 0));

            float forwardaxis = (Raylib.IsKeyDown(KeyboardKey.W) - Raylib.IsKeyDown(KeyboardKey.S));
            float rightaxis = (Raylib.IsKeyDown(KeyboardKey.A) - Raylib.IsKeyDown(KeyboardKey.D));
            float upaxis = (Raylib.IsKeyDown(KeyboardKey.Space) - Raylib.IsKeyDown(KeyboardKey.LeftShift));

            var vspd = spd;

            if (noclipping)
            {
                vspd *= 8;
            }

            float hsp = camforward.X * forwardaxis * vspd;
            float vsp = camforward.Y * forwardaxis * vspd;
            hsp += camright.X * rightaxis * vspd;
            vsp += camright.Y * rightaxis * vspd;

            if (!noclipping) { Velocity.Z -= gravity; } else { Velocity.Z = upaxis * vspd; }
            Velocity.X = hsp;
            Velocity.Y = vsp;

            setblocklookingat();

            if (grounded && Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Velocity.Z = 12f;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                unstuck();
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && mouse_locked)
            {
                if (focused_on_block)
                {
                    hit_anim += 20 * deltatime;
                    block_health -= 3 * deltatime;
                    if (block_health <= 0)
                    {
                        block_health = 0;
                        hit_anim = 0;
                        focused_on_block = false;
                        world.BreakBlock(focused_block_pos);
                    }
                }
                else
                {
                    hit_anim = 0;
                }
            }
            else
            {
                hit_anim = 0;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right) && mouse_locked)
            {
                if (holding_something)
                {
                    InventorySlot slot = inventory.GetSlot(selected_slot);
                    Item item = Globals.ItemDefinitions[slot.item];

                    item.OnRightClicked(this, slot, inventory, world);
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                invpanel.visible = !invpanel.visible;
                hotbarpanel.visible = !invpanel.visible;
                mouse_locked = !invpanel.visible;

                if (invpanel.visible)
                {
                    Raylib.EnableCursor();
                }
                else
                {
                    Raylib.DisableCursor();
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.V))
            {
                noclipping = !noclipping;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.F))
            {
                inventory.AddItem((ItemType)Raylib.GetRandomValue(0, (int)ItemType.Count-1), 1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.F4))
            {
                Raylib.ToggleFullscreen();
            }

            if (!noclipping)
            {
                base.Update(deltatime); // collision checking
            }
            else
            {
                Position += Velocity * deltatime;
            }

            if ((Velocity.X != 0 || Velocity.Y != 0) && grounded)
            {
                float spd = new Vector2(Velocity.X, Velocity.Y).Length();
                bob_anim += spd*1.75f*deltatime;
            }
            else
            {
                bob_anim = 0;
            }

            //GUI

            GUIroot.Update();

            int scroll = (int)Raylib.GetMouseWheelMove();
            if (scroll != 0)
            {
                scroll = Math.Sign(scroll);

                SelectHotbar(selected_hotbar + scroll);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.One))
            {
                SelectHotbar(0);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Two))
            {
                SelectHotbar(1);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Three))
            {
                SelectHotbar(2);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Four))
            {
                SelectHotbar(3);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Five))
            {
                SelectHotbar(4);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Six))
            {
                SelectHotbar(5);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Seven))
            {
                SelectHotbar(6);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Eight))
            {
                SelectHotbar(7);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Nine))
            {
                SelectHotbar(8);
            }
        }
        public override void Draw()
        {
            if (last_raycast != null)
            {
                var v = (1 - (block_health / max_block_health));
                v *= 5;
                v += 1;
                Raylib.DrawCubeWiresV(last_raycast.BlockPosition.to_vector3() + (Vector3.One * 0.5f), last_raycast.Collider.Size * 1.05f, Color.White);
                Raylib.DrawSphere(last_raycast.Hit, 0.01f * v, Color.White);

                if (block_health < max_block_health)
                {
                    Matrix4x4 crackmatrix = MatrixScale(0.51f,0.51f,0.51f);
                    crackmatrix = MatrixMultiply(crackmatrix, MatrixTranslate(focused_block_pos.x + 0.5f, focused_block_pos.y + 0.5f, focused_block_pos.z + 0.5f));
                    ModelHandler.DrawBlock(crack_anim[(int)((block_health / max_block_health) * 5)], crackmatrix);
                }
            }

            //camlookpitch * 0.0174533f

            if (holding_something)
            {
                Matrix4x4 mat = MatrixRotateX(MathF.Sin(hit_anim) * 0.3f);
                mat = MatrixMultiply(mat, MatrixTranslate(3.5f, 3.5f, -2f + (MathF.Abs(MathF.Sin(bob_anim)) * 0.25f)));
                mat = MatrixMultiply(mat, MatrixMultiply(MatrixRotateXYZ(new Vector3((camlookpitch + camdelta.X) * 0.0174533f, 0, 0)), MatrixRotateXYZ(new Vector3(0, 0, (camlookyaw + camdelta.Y) * 0.0174533f))));
                mat = MatrixMultiply(mat, MatrixScale(0.05f, 0.05f, 0.05f));
                mat = MatrixMultiply(mat, MatrixTranslate(world.cam.Position.X, world.cam.Position.Y, world.cam.Position.Z));

                ModelHandler.DrawItem(holding_itemtype, mat);
            }
        }

        public override void DrawGui()
        {
            Raylib.DrawText((int)Position.X + ", " + (int)Position.Y + ", " + (int)Position.Z, 32, 256, 24, Color.White);

            GUIroot.Draw();

            //Item itemdef = Globals.ItemDefinitions[ItemType.TestPickaxe];

            //Raylib.DrawTexturePro(TextureHandler.block_atlas.Texture, TextureHandler.GetTextureUV(itemdef.texture).ToRectangle(), new Rectangle(0, 0, 64, 64), new Vector2(0,0), 15, Color.White);
            
            //invgui.Draw();
            //Raylib.DrawTexture(viewmodel_rendertexture.Texture, 0, 0, Color.White);
        }

        //player entity spesific
        private Vector3 GetCameraForward()
        {
            float rad = (MathF.PI / 180);
            float radyaw = (camlookyaw) * rad;
            float radpitch = (camlookpitch) * rad;

            return new Vector3(
                -(1 * MathF.Sin(radyaw) * MathF.Cos(radpitch)),
                (1 * MathF.Cos(radyaw) * MathF.Cos(radpitch)),
                (1 * MathF.Sin(radpitch))
            );
        }
        private Vector3 GetCameraRight()
        {
            float rad = (MathF.PI / 180);
            float radyaw = (camlookyaw + 90) * rad;
            float radpitch = (camlookpitch) * rad;

            return new Vector3(
                -(1 * MathF.Sin(radyaw)),
                (1 * MathF.Cos(radyaw)),
                0
            );
        }

        private Vector3 GetCameraUp()
        {
            float rad = (MathF.PI / 180);
            float radyaw = (camlookyaw) * rad;
            float radpitch = (camlookpitch - 90) * rad;

            return new Vector3(
                -(1 * MathF.Sin(radyaw) * MathF.Cos(radpitch)),
                (1 * MathF.Cos(radyaw) * MathF.Cos(radpitch)),
                1 * MathF.Sin(radpitch)
            );
        }

        public void PlaceBlock(BlockType block_to_place)
        {
            if (focused_on_block && last_raycast != null)
            {
                BoxCollider blockcollider = world.GetBlockCollider(focused_block_pos + last_raycast.Normal);
                if (!blockcollider.CollidingWith(collider))
                {
                    world.PlaceBlock(focused_block_pos + last_raycast.Normal, block_to_place);
                }
            }
        }

        private void UpdateCamera(float deltatime)
        {
            if (mouse_locked)
            {
                Vector2 mousedelta = Raylib.GetMouseDelta();

                last_camlookpitch = camlookpitch;
                last_camlookyaw = camlookyaw;

                camlookyaw -= mousedelta.X * mousesensitivity * deltatime;
                camlookpitch -= mousedelta.Y * mousesensitivity * deltatime;

                if (camlookpitch <= -89)
                {
                    camlookpitch = -89;
                }
                if (camlookpitch >= 89)
                {
                    camlookpitch = 89;
                } 
            }

            camdelta.X = Raymath.Lerp(camdelta.X, (camlookpitch - last_camlookpitch) * 0.5f, 5 * deltatime);
            camdelta.Y = Raymath.Lerp(camdelta.Y, (camlookyaw - last_camlookyaw) * 0.5f, 5 * deltatime);

            float rad = (MathF.PI / 180);
            float radyaw = camlookyaw * rad;
            float radpitch = camlookpitch * rad;

            Vector3 campos = Position + new Vector3(0, 0, 0.8f);
            Vector3 topos = new Vector3(
                campos.X - (5 * MathF.Sin(radyaw) * MathF.Cos(radpitch)),
                campos.Y + (5 * MathF.Cos(radyaw) * MathF.Cos(radpitch)),
                campos.Z + (5 * MathF.Sin(radpitch))
            );
            world.cam.Position = campos;
            world.cam.Target = topos;
        }
    }
}
