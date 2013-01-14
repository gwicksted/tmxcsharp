namespace TmxCSharp.Models
{
    public class MapLayer
    {
        public MapLayer(string name, int width, int height)
        {
            Name = name;

            Width = width;

            Height = height;

            TileIds = new int[Height, Width];
        }

        public string Name { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int[,] TileIds { get; private set; }
    }
}