using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public struct Int3 //used for index's for chunks mostly
    {
        public int x;
        public int y;
        public int z;

        public Int3(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public static Int3 operator +(Int3 yc1, Int3 yc2)
        {
            return new Int3() { x = yc1.x + yc2.x, y = yc1.y + yc2.y, z = yc1.z + yc2.z };
        }

        public static Int3 operator -(Int3 yc1, Int3 yc2)
        {
            return new Int3() { x = yc1.x - yc2.x, y = yc1.y - yc2.y, z = yc1.z - yc2.z };
        }

        public static Int3 operator -(Int3 yc1)
        {
            return new Int3() { x = -yc1.x, y = -yc1.y, z = -yc1.z };
        }

        public static Int3 operator *(Int3 yc1, Int3 yc2)
        {
            return new Int3() { x = yc1.x * yc2.x, y = yc1.y * yc2.y, z = yc1.z * yc2.z };
        }

        public static Int3 operator *(Int3 yc1, int by)
        {
            return new Int3() { x = yc1.x * by, y = yc1.y * by, z = yc1.z * by };
        }

        public static Int3 operator /(Int3 yc1, Int3 yc2)
        {
            return new Int3() { x = yc1.x / yc2.x, y = yc1.y / yc2.y, z = yc1.z / yc2.z };
        }

        public static Int3 operator /(Int3 yc1, int by)
        {
            return new Int3() { x = yc1.x / by, y = yc1.y / by, z = yc1.z / by };
        }

       public static bool operator ==(Int3 yc1, Int3 yc2)
        {
            return (yc1.x == yc2.x) && (yc1.y == yc2.y) && (yc1.z == yc2.z);
        }

        public static bool operator !=(Int3 yc1, Int3 yc2)
        {
            return (yc1.x != yc2.x) || (yc1.y != yc2.y) || (yc1.z != yc2.z);
        }

        public Vector3 to_vector3()
        {
            return new Vector3(x, y, z);
        }

        public void Print()
        {
            Console.WriteLine("(" + x + ", " + y + ", " + z + ")");
        }
    }
}
