using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

namespace blockengine.Entitys
{
    public class DroppedItemEntity : LivingEntity
    {
        private float spd = 5f;
        private Vector3 dir;
        private bool zooping = false;
        public DroppedItemEntity(World _world, string _Name, Vector3 _position) : base(_world, _Name, _position)
        {

        }

        public override void Draw()
        {
            Raylib.DrawCube(Position, 0.15f, 0.15f, 0.15f, Color.White);
        }

        public override void Start()
        {
            dir = new Vector3(Raylib.GetRandomValue(-25, 25) / 25f, Raylib.GetRandomValue(-25, 25) / 25f, Raylib.GetRandomValue(-25, 25) / 25f);
        }
        public override void Update(float deltatime)
        {
            Entity? player = world.GetFocusEntity();
            if (player != null)
            {
                if (!zooping)
                {
                    var _dist = Vector3.Distance(Position, player.Position);
                    if (_dist < 5)
                    {
                        zooping = true;
                    }

                    Velocity += new Vector3(0, 0, -1) * 0.25f;
                    base.Update(deltatime);
                }
                else
                {
                    var todir = player.Position - Position;
                    Position += dir * spd * deltatime;
                    spd += 5f * deltatime;
                    dir = Raymath.Vector3Lerp(dir, todir, 10f * deltatime);

                    if (dir.Length() < 0.5f)
                    {
                        world.DestroyEntity(Id);
                    }
                }
            }
        }

    }
}
