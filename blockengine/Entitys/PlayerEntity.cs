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

namespace blockengine.Entitys
{
    public class PlayerEntity : LivingEntity
    {
        private float camlookyaw = 0;
        private float camlookpitch = 0;
        private float last_camlookyaw = 0;
        private float last_camlookpitch = 0;
        private Vector2 camdelta;
        public Camera3D cam;

        private float gravity = 0.25f;
        private float mousesensitivity = 30f;
        private float spd = 4f;
        private BlockType block_to_place = BlockType.BrownStoneBlock;
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
        private InventoryGUI invgui;
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


            invgui = new InventoryGUI();
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

            float hsp = camforward.X * forwardaxis * spd;
            float vsp = camforward.Y * forwardaxis * spd;
            hsp += camright.X * rightaxis * spd;
            vsp += camright.Y * rightaxis * spd;

            if (!noclipping) { Velocity.Z -= gravity; } else { Velocity.Z = upaxis * spd; }
            Velocity.X = hsp;
            Velocity.Y = vsp;

            setblocklookingat();

            if (grounded && Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Velocity.Z = 12f;
            }

            int scroll = (int)Raylib.GetMouseWheelMove();
            if (scroll != 0)
            {
                scroll = Math.Sign(scroll);

                block_to_place += scroll;
                if (block_to_place >= BlockType.BlockCount)
                {
                    block_to_place = 0;
                }
                if (block_to_place < 0)
                {
                    block_to_place = BlockType.BlockCount - 1;
                }

                var blockdef = Globals.BlockDefinitions[block_to_place];

                scroll = 0;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                unstuck();
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && !invgui.inv_showing)
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

            if (Raylib.IsMouseButtonPressed(MouseButton.Right) && !invgui.inv_showing)
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

            if (Raylib.IsKeyPressed(KeyboardKey.E))
            {
                invgui.ToggleInventory();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.V))
            {
                noclipping = !noclipping;
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

            //invgui.Update();
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

            Matrix4x4 mat = MatrixRotateX(MathF.Sin(hit_anim)*0.3f);
            mat = MatrixMultiply(mat, MatrixTranslate(3.5f,3.5f, -2f + (MathF.Abs(MathF.Sin(bob_anim)) * 0.25f) ));
            mat = MatrixMultiply(mat, MatrixMultiply(MatrixRotateXYZ(new Vector3((camlookpitch + camdelta.X) * 0.0174533f, 0, 0)), MatrixRotateXYZ(new Vector3(0, 0, (camlookyaw + camdelta.Y) * 0.0174533f))));
            mat = MatrixMultiply(mat, MatrixScale(0.05f, 0.05f, 0.05f));
            mat = MatrixMultiply(mat, MatrixTranslate(world.cam.Position.X, world.cam.Position.Y, world.cam.Position.Z));

            ModelHandler.DrawBlockType(block_to_place, mat);
        }

        public override void DrawGui()
        {
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

        private void UpdateCamera(float deltatime)
        {
            if (!invgui.inv_showing)
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
