using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TmxCSharp.Models;

namespace TmxCSharp.Loader
{
    /// <summary>
    /// Used to load .tmx files that were created with Tiled: http://www.mapeditor.org/
    /// </summary>
    public static class TmxLoader
    {
        private const string SupportedVersion = "1.0";
        private const string SupportedOrientation = "orthogonal";

        public static TileMap Parse(string fileName)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                return Parse(XDocument.Load(stream));
            }
        }

        public static TileMap Parse(XDocument document)
        {
            XElement map = document.Element("map");

            if (map == null)
            {
                throw new InvalidDataException("Missing 'map' element");
            }

            AssertRequirements(map);

            TileMapSize tileMapSize = TileMapSizeLoader.LoadTileMapSize(map);

            IList<TileSet> tileSets = TileSetLoader.LoadTileSets(map.Elements("tileset"));

            TileIdLoader tileIdLoader = new TileIdLoader(tileMapSize);

            IList<MapLayer> layers = MapLayerLoader.LoadMapLayers(map, tileIdLoader);

            return new TileMap(tileMapSize, tileSets, layers);
        }


        private static void AssertRequirements(XElement map)
        {
            string version = (string) map.Attribute("version");

            if (version != SupportedVersion)
            {
                throw new InvalidDataException(
                    string.Format("Unsupported map version '{0}'. Only version '{1}' is supported.",
                                  version,
                                  SupportedVersion));
            }

            string orientation = (string) map.Attribute("orientation");

            if (orientation != SupportedOrientation)
            {
                throw new InvalidDataException(
                    string.Format("Unsupported orientation '{0}'. Only '{1}' orientation is supported.",
                                  orientation,
                                  SupportedOrientation));
            }
        }
    }
}