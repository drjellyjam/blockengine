using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine.Entitys
{
    public abstract class Entity
    {
        protected string Id;
        public string Name;
        public Vector3 Position;
        protected World world;
        public Entity(World _world, string _Name, Vector3 _position)
        {
            world = _world;
            Id = Guid.NewGuid().ToString();
            Name = _Name;
            Position = _position;
        }

        public string GetID()
        {
            return Id;
        }
        public virtual void DrawGui() { }
        public virtual void Draw() { } // on draw
        public virtual void Tick() { } // on world tick
        public virtual void Update(float deltatime) { } // on frame
        public virtual void Start() { } // on entity spawn
        public virtual void End() { }
    }
}
