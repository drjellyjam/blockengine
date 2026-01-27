using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace blockengine.Entitys
{
    public abstract class LivingEntity : Entity
    {
        public int max_health = 100;
        public int health = 100;
        public Vector3 Velocity;
        public BoxCollider collider = new BoxCollider(Vector3.Zero, new Vector3(-1 / 2, -1 / 2, -1 / 2), new Vector3(1 / 2, 1 / 2, 1 / 2));

        public bool grounded = false;
        public LivingEntity(World _world, string _Name, Vector3 _position) : base(_world, _Name, _position)
        {

        }

        public void TakeDamage(Entity _damager,int _damage)
        {
            if (health > 0)
            {
                health -= _damage;
                if (health <= 0)
                {
                    Died(_damager);
                }
            }
        }
        public void Died(Entity _damager)
        {

            world.DestroyEntity(Id);
        }
        public override void Update(float deltatime)
        {
            grounded = false;
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
                    grounded = true;
                }
                Velocity.Z = 0;
            }
            Position += Velocity * deltatime;
        }

        public bool CheckCollision(Vector3 thisVelocity, float dt)
        {
            Vector3 topos = Position + (thisVelocity * dt);
            collider.Position = topos;

            if (!world.ChunkExists(Globals.WorldPosToChunkPos(topos)))
            {
                return true;
            }

            for (int z = (int)Math.Floor(collider.Min.Z - 1); z < (int)Math.Ceiling(collider.Max.Z + 1); z++)
            {
                for (int y = (int)Math.Floor(collider.Min.Y - 1); y < (int)Math.Ceiling(collider.Max.Y + 1); y++)
                {
                    for (int x = (int)Math.Floor(collider.Min.X - 1); x < (int)Math.Ceiling(collider.Max.X + 1); x++)
                    {
                        Int3 bp = new Int3((int)topos.X + x, (int)topos.Y + y, (int)topos.Z + z);
                        ChunkBlock? block = world.GetBlock(bp);
                        if (block != null)
                        {
                            Block blockdef = block.GetBlockDef();
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

        public void SetCollider(Vector3 _pos, Vector3 _min, Vector3 _max)
        {
            collider.Position = _pos;
            collider.Min = _min;
            collider.Max = _max;
        }
    }
}
