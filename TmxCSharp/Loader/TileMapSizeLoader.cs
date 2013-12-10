using System;
using System.Xml.Linq;
using TmxCSharp.Models;

namespace TmxCSharp.Loader
{
    public static class TileMapSizeLoader
    {
        public static TileMapSize LoadTileMapSize(XElement map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            return new TileMapSize(
                (int)map.Attribute("width"),
                (int)map.Attribute("height"),
                (int)map.Attribute("tilewidth"),
                (int)map.Attribute("tileheight"));
        }
    }
}
