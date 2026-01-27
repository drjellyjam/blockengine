using blockengine;
using blockengine.Entitys;
using Raylib_cs;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;

namespace BlockEngine;
class Program
{
    private static List<Int3> list = new List<Int3>();
    private static void flood(List<Int3> poslist,int d = 4,bool print = false)
    {
        if (d <= 0)
        {
            return;
        }

        var _newlist = new List<Int3>();
        foreach (Int3 pos in poslist)
        {
            if (!list.Contains(pos))
            {
                list.Add(pos);
                if (print)
                {
                    Console.WriteLine("new Int3(" + pos.x + "," + pos.y + "," + pos.z + "),");
                }
                foreach (Int3 norm in Globals.around_positions)
                {
                    var _npos = pos + norm;
                    if (!list.Contains(_npos))
                    {
                        _newlist.Add(_npos);

                    }
                }
            }
        }
        flood(_newlist, d - 1, print);
    }
    public static void Main()
    {
        //Console.WriteLine("precalculated flood positions");
        //flood(new List<Int3>(){ new Int3(0,0,0) },2);
        //Console.WriteLine("");
        //Console.WriteLine("Done " + list.Count);
        //return;
        
        bool hacky_fix_draw_glitch = true;
        bool draw_debug = false;

        if (!Directory.Exists("Saves"))
        {
            Directory.CreateDirectory("Saves");
        }

        Raylib.InitWindow(1280, 720, "Block Engine");

        World world = new World(new WorldInfo("test world",67));
        
        world.AddEntity(new PlayerEntity(world, "Player", Vector3.Zero),true);

        float max_frames = 0.1f;
        float bi = max_frames;

        ModelHandler.LoadModels();
        TextureHandler.CreateAtlasTextures();

        
        Task.Run(() =>
        {
           world.GenerateArea();
        });
        

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F3))
            {
                draw_debug = !draw_debug;
            }

            //update

            world.UpdateEntities();

            bi -= 1 * Raylib.GetFrameTime();
            if (bi <= 0) {
                bi = max_frames;

                world.UploadChunks();
            }

            //drawing

            Raylib.BeginDrawing();

            //behind 3d

            Raylib.ClearBackground(Color.Gray);

            //3d

            Raylib.BeginMode3D(world.cam);

            if (hacky_fix_draw_glitch)
            {
                hacky_fix_draw_glitch = false;
                Raylib.DrawCube(Vector3.Zero, 1, 1, 1, Color.White);
            }

            world.DrawAllChunks();
            world.DrawEntities();

            Raylib.EndMode3D();

            //ui

            Raylib.DrawFPS(10, 10);

            //Raylib.DrawTexture(TextureHandler.block_atlas.Texture, 0, 0, Color.Red);

            Raylib.EndDrawing();
        }

        lock (world)
        {
            world.Cleanup();
        }
        TextureHandler.Cleanup();

        Raylib.CloseWindow();
    }
}