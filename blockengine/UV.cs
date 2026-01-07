using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public struct UV
    {
        public float XMIN;
        public float YMIN;
        public float XMAX;
        public float YMAX;
        public UV(float xmin,float ymin,float xmax,float ymax) {
            XMIN = xmin;
            YMIN = ymin;
            XMAX = xmax;
            YMAX = ymax;
        }
    }
}
