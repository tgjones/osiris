using System;

namespace ShapeVisualizerDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ShapeVisualizerGame game = new ShapeVisualizerGame())
            {
                game.Run();
            }
        }
    }
#endif
}

