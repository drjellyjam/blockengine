using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
using System.Diagnostics;

namespace blockengine.Entitys
{
    public class PlayerEntity : LivingEntity
    {
        private float camlookyaw = 0;
        private float camlookpitch = 0;
        public Camera3D cam;

        private float gravity = 0.25f;
        private float mousesensitivity = 30f;
        private float spd = 4f;
        private BlockType block_to_place = BlockType.BrownStoneBlock;
        public PlayerEntity(World _world, string _Name, Vector3 _position) : base(_world, _Name, _position)
        {
            max_health = 100;
            health = max_health;

            var collider_width = 0.75f;
            var collider_height = 1.5f;
            SetCollider(Vector3.Zero, new Vector3(-collider_width / 2, -collider_width / 2, -collider_height / 2), new Vector3(collider_width / 2, collider_width / 2, collider_height / 2));
        }

        public override void Start()
        {
            world.cam.Position = Position;

            Raylib.DisableCursor();
        }

        public override void Update(float deltatime)
        {
            UpdateCamera(deltatime);

            Vector3 camforward = Vector3.Normalize(GetCameraForward() * new Vector3(1, 1, 0));
            Vector3 camright = Vector3.Normalize(GetCameraRight() * new Vector3(1, 1, 0));

            float forwardaxis = (Raylib.IsKeyDown(KeyboardKey.W) - Raylib.IsKeyDown(KeyboardKey.S));
            float rightaxis = (Raylib.IsKeyDown(KeyboardKey.A) - Raylib.IsKeyDown(KeyboardKey.D));

            float hsp = camforward.X * forwardaxis * spd;
            float vsp = camforward.Y * forwardaxis * spd;
            hsp += camright.X * rightaxis * spd;
            vsp += camright.Y * rightaxis * spd;

            Velocity.Z -= gravity;
            Velocity.X = hsp;
            Velocity.Y = vsp;

            if (grounded && Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Velocity.Z = 12f;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                RaycastResult? result = world.Raycast(world.cam.Position, GetCameraForward() * 8);
                if (result != null)
                {
                    world.BreakBlock(result.BlockPosition);
                }
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                RaycastResult? result = world.Raycast(world.cam.Position, GetCameraForward() * 8);
                if (result != null)
                {
                    Block? blockplacedefinition = Globals.BlockDefinitions[block_to_place];
                    if (blockplacedefinition != null)
                    {
                        BoxCollider placecollider = blockplacedefinition.collider;
                        placecollider.Position = (result.BlockPosition + result.Normal).to_vector3() + (Vector3.One * 0.5f);
                        if (!placecollider.CollidingWith(collider))
                        {
                            world.PlaceBlock(result.BlockPosition + result.Normal, BlockType.MineBlock);
                        }
                    }
                }
            }

            base.Update(deltatime);
        }
        public override void Draw()
        {
            RaycastResult? result = world.Raycast(world.cam.Position, GetCameraForward() * 8);
            if (result != null)
            {
                Raylib.DrawCubeWiresV(result.BlockPosition.to_vector3() + (Vector3.One * 0.5f), result.Collider.Size * 1.05f, Color.White);
                Raylib.DrawSphere(result.Hit, 0.01f, Color.White);
            }
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
        private void UpdateCamera(float deltatime)
        {
            Vector2 mousedelta = Raylib.GetMouseDelta();
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

            float rad = (MathF.PI / 180);
            float radyaw = camlookyaw * rad;
            float radpitch = camlookpitch * rad;

            Vector3 campos = Position + new Vector3(0, 0, 1);
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
