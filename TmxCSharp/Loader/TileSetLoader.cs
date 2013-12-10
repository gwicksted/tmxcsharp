using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TmxCSharp.Models;

namespace TmxCSharp.Loader
{
    internal static class TileSetLoader
    {
        public static IList<TileSet> LoadTileSets(IEnumerable<XElement> tileSets)
        {
            return tileSets.Select(tileSet => GetTileSet(tileSet, GetTileSetImage(tileSet.Element("image")))).ToList();
        }

        private static TileSet GetTileSet(XElement tileSetDefinition, TileSetImage tileSetImage)
        {
            if (tileSetDefinition == null)
            {
                throw new ArgumentNullException("tileSetDefinition");
            }

            if (tileSetImage == null)
            {
                throw new ArgumentNullException("tileSetImage");
            }

            return new TileSet((int) tileSetDefinition.Attribute("firstgid"),
                               (string) tileSetDefinition.Attribute("name"),
                               (int) tileSetDefinition.Attribute("tilewidth"),
                               (int) tileSetDefinition.Attribute("tileheight"),
                               tileSetImage);
        }

        private static TileSetImage GetTileSetImage(XElement image)
        {
            if (image == null)
            {
                throw new InvalidDataException("Tile set missing image");
            }

            return new TileSetImage((string) image.Attribute("source"),
                                    (int) image.Attribute("width"),
                                    (int) image.Attribute("height"));
        }
    }
}
