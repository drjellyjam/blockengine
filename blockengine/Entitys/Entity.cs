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
        public int Id;
        public string Name;
        public Vector3 Position;
        protected World world;
        public Entity(World _world, int _Id, string _Name, Vector3 _position)
        {
            world = _world;
            Id = _Id;
            Name = _Name;
            Position = _position;
        }

        public abstract void DrawGui();
        public abstract void Draw();
        public abstract void Tick();
        public abstract void Start();
    }
}
