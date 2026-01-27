using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;

namespace blockengine
{
    public static class ModelHandler
    {
        private static Dictionary<string, ParsedOBJ> models = new Dictionary<string, ParsedOBJ>();

        public static ParsedOBJ? GetModel(string modeldir)
        {
            if (models.ContainsKey(modeldir)) { return models[modeldir]; }

            return null;
        }

        public static void LoadModels()
        {
            string[] model_files = Directory.GetFiles("Assets/Models/");

            foreach (string modeldir in model_files)
            {
                string[] lines = File.ReadAllLines(modeldir);
                ParsedOBJ parsed = new ParsedOBJ();

                foreach (string line in lines)
                {
                    string[] parts = line.Split(" ");
                    //Console.WriteLine(line);
                    switch (parts[0])
                    {
                        case "v": //POSITION
                            float x = Convert.ToSingle(parts[1]);
                            float y = Convert.ToSingle(parts[2]);
                            float z = Convert.ToSingle(parts[3]);
                            parsed.AddVertex(new Vector3(x, y, z));

                            
                            break;

                        case "vn": //NORMAL
                            float xn = Convert.ToSingle(parts[1]);
                            float yn = Convert.ToSingle(parts[2]);
                            float zn = Convert.ToSingle(parts[3]);
                            parsed.AddNormal(new Vector3(xn, yn, zn));
                            
                            break;

                        case "vt": //UV
                            float xuv = Convert.ToSingle(parts[1]);
                            float yuv = Convert.ToSingle(parts[2]);
                            parsed.AddUV(new Vector2(xuv, yuv));

                            
                            break;

                        case "f": //FACE
                            for (int i = 1; i<=3; i++)
                            {
                                string vertex = parts[i];
                                Console.WriteLine(vertex);
                                string[] vertex_parts = vertex.Split("/");
                                
                                //Console.WriteLine(vertex_parts[0] + "/" + vertex_parts[1] + "/" + vertex_parts[2]);

                                int v = int.Parse(vertex_parts[0])-1;
                                int vt = int.Parse(vertex_parts[1])-1;
                                int vn = int.Parse(vertex_parts[2])-1;

                                parsed.AddFace(new Int3(v, vt, vn));
                            }
                            break;
                    }
                }

                models[modeldir] = parsed;
                Console.WriteLine("Parsed: " + modeldir + " (" + parsed.face_count + ")");
            }
        }
    }
}
