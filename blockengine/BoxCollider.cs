using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class BoxCollider
    {
        public Vector3 Position;
        public Vector3 Size;
        public Vector3 Min;
        public Vector3 Max;

        public BoxCollider(Vector3 _pos, Vector3 min, Vector3 max) 
        {
            Position = _pos;
            Min = min;
            Max = max;
            Size = max - min;
        }

        public Vector3[] GetCorners()
        {
            return new Vector3[8] {
            new Vector3(Min.X, Min.Y, Min.Z),new Vector3(Max.X,Min.Y,Min.Z), new Vector3(Min.X, Max.Y, Min.Z), new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),new Vector3(Max.X,Min.Y,Max.Z), new Vector3(Min.X, Max.Y, Max.Z), new Vector3(Max.X, Max.Y, Max.Z)};
        }

        public Vector3[] GetCornersAndCenter()
        {
            return new Vector3[9] {
            Max - (Min/2),new Vector3(Min.X, Min.Y, Min.Z),new Vector3(Max.X,Min.Y,Min.Z), new Vector3(Min.X, Max.Y, Min.Z), new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),new Vector3(Max.X,Min.Y,Max.Z), new Vector3(Min.X, Max.Y, Max.Z), new Vector3(Max.X, Max.Y, Max.Z)};
        }


        public bool CollidingWith(BoxCollider other)
        {
            return
                (Position.X + Min.X < other.Position.X + other.Max.X) &&
                (Position.Y + Min.Y < other.Position.Y + other.Max.Y) &&
                (Position.Z + Min.Z < other.Position.Z + other.Max.Z) &&
                (Position.X + Max.X > other.Position.X + other.Min.X) &&
                (Position.Y + Max.Y > other.Position.Y + other.Min.Y) &&
                (Position.Z + Max.Z > other.Position.Z + other.Min.Z);
        }
    }
}
