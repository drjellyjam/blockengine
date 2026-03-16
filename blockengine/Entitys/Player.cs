using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine.Entitys
{
    /*
     * ####################################### DEPRECATED ##########################################
     * 
    public class Player
    {
        public Vector3 Velocity;
        public Vector3 Position;
        public Camera3D cam;
        public float camlookyaw = 0;
        public float camlookpitch = 0;
        public BoxCollider collider;
        public World world;
        string block_to_place = "BLUE_ORE";
        float gravity = 0.25f;
        public bool noclipping = false;

        public Player(World currentworld) {
            world = currentworld;

            Velocity = new Vector3();
            Position = new Vector3();
            cam = new Camera3D();
            cam.Position = Position;
            cam.Target = Vector3.Zero;
            cam.Up = new Vector3(0, 0, 1);
            cam.FovY = 70f;
            cam.Projection = CameraProjection.Perspective;

            float collider_width = 0.75f;
            float collider_height = 1.5f;
            collider = new BoxCollider(Vector3.Zero,new Vector3(-collider_width/ 2, -collider_width/ 2, -collider_height/ 2), new Vector3(collider_width / 2, collider_width / 2, collider_height / 2));

            Raylib.DisableCursor();
        }

        public void UpdateCamera()
        {
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
            cam.Position = campos;
            cam.Target = topos;
        }

        public bool CheckCollision(Vector3 thisVelocity,float dt)
        {
            Vector3 topos = Position + (thisVelocity * dt);
            collider.Position = topos;

            for (int z = (int)Math.Floor(collider.Min.Z-1); z< (int)Math.Ceiling(collider.Max.Z+1); z++)
            {
                for (int y = (int)Math.Floor(collider.Min.Y - 1); y < (int)Math.Ceiling(collider.Max.Y + 1); y++)
                {
                    for (int x = (int)Math.Floor(collider.Min.X - 1); x < (int)Math.Ceiling(collider.Max.X + 1); x++)
                    {
                        Int3 bp = new Int3( (int)topos.X + x, (int)topos.Y + y, (int)topos.Z + z);
                        BlockType? block = world.GetBlock(bp);
                        if (block != null)
                        {
                            Block? blockdef = Globals.BlockDefinitions[(BlockType)block];
                            if (blockdef.IsExists() && !blockdef.IsNonSolid())
                            {
                                BoxCollider blockcollider = world.GetBlockCollider(bp);
                                if (collider.CollidingWith(blockcollider))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public Vector3 GetCameraForward()
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
        public Vector3 GetCameraRight()
        {
            float rad = (MathF.PI / 180);
            float radyaw = (camlookyaw+90) * rad;
            float radpitch = (camlookpitch) * rad;

            return new Vector3(
                -(1 * MathF.Sin(radyaw)),
                (1 * MathF.Cos(radyaw)),
                0
            );
        }
        public void Update()
        {
            float deltatime = MathF.Min(Raylib.GetFrameTime(),1f);
            float mousesensitivity = 30f;

            Vector2 mousedelta = Raylib.GetMouseDelta();
            camlookyaw -= mousedelta.X * mousesensitivity * deltatime;
            camlookpitch -= mousedelta.Y * mousesensitivity * deltatime;
            UpdateCamera();

            Vector3 camforward = Vector3.Normalize(GetCameraForward()*new Vector3(1,1,0));
            Vector3 camright = Vector3.Normalize(GetCameraRight() * new Vector3(1, 1, 0));

            float forwardaxis = (Raylib.IsKeyDown(KeyboardKey.W) - Raylib.IsKeyDown(KeyboardKey.S));
            float rightaxis = (Raylib.IsKeyDown(KeyboardKey.A) - Raylib.IsKeyDown(KeyboardKey.D));

            float spd = 4f;

            if (noclipping)
            {
                spd = 8f;
            }

            float hsp = camforward.X * forwardaxis * spd;
            float vsp = camforward.Y * forwardaxis * spd;
            hsp += camright.X * rightaxis * spd;
            vsp += camright.Y * rightaxis * spd;

            Velocity.Z -= gravity;
            Velocity.X = hsp;
            Velocity.Y = vsp;
            
            if (noclipping)
            {
                Velocity.Z = 0;
                if (Raylib.IsKeyDown(KeyboardKey.Space))
                {
                    Velocity.Z = 8f;
                }
                if (Raylib.IsKeyDown(KeyboardKey.LeftShift))
                {
                    Velocity.Z = -8f;
                }
            }
            else
            {
                bool X_colliding = CheckCollision(new Vector3(Velocity.X, 0, 0), deltatime);
                if (X_colliding)
                {
                    Velocity.X = 0;
                }
                bool Y_colliding = CheckCollision(new Vector3(0, Velocity.Y, 0), deltatime);
                if (Y_colliding)
                {
                    Velocity.Y = 0;
                }
                bool grav_colliding = CheckCollision(new Vector3(0, 0, Velocity.Z), deltatime);
                if (grav_colliding)
                {
                    if (Velocity.Z < 0)
                    {
                        Velocity.Z = 0;
                    }
                    else
                    {
                        Velocity.Z = -Velocity.Z;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Space) && Velocity.Z == 0)
                    {
                        Velocity.Z = 12f;
                    }
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.V))
            {
                noclipping = !noclipping;
            }

            Position += Velocity * deltatime;

            int scroll = (int)Raylib.GetMouseWheelMove();
            if (scroll != 0)
            {
                scroll = Math.Sign(scroll);

                //block_to_place += scroll;
                
                scroll = 0;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                RaycastResult? result = world.Raycast(cam.Position, GetCameraForward() * 8);
                if (result != null)
                {
                    world.SetBlock(result.BlockPosition, new AirBlock());
                }
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                RaycastResult? result = world.Raycast(cam.Position, GetCameraForward() * 8);
                if (result != null)
                {
                    BlockDefinition? blockplacedefinition = Globals.BlockDefinitions[block_to_place];
                    if (blockplacedefinition != null)
                    {
                        BoxCollider placecollider = blockplacedefinition.Collider;
                        placecollider.Position = (result.BlockPosition + result.Normal).to_vector3() + (Vector3.One * 0.5f);
                        if (!placecollider.CollidingWith(collider))
                        {
                            world.SetBlock(result.BlockPosition + result.Normal, new AirBlock());
                        }
                    }
                }
            }

            //Raylib.DrawCubeV(collider.Position, collider.Size, Color.Red);
        }

        public void Draw()
        {
            RaycastResult? result = world.Raycast(cam.Position, GetCameraForward() * 8);
            if (result != null)
            {
                Raylib.DrawCubeWiresV(result.BlockPosition.to_vector3() + (Vector3.One * 0.5f), result.Collider.Size * 1.05f, Color.White);
                Raylib.DrawSphere(result.Hit, 0.1f, Color.Red);
            }
        }
    }
    */
}