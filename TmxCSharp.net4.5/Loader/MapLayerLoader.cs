using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TmxCSharp.net45.Models;

namespace TmxCSharp.net45.Loader
{
    internal static class MapLayerLoader
    {
        public static IList<MapLayer> LoadMapLayers(XElement map, TileIdLoader tileIdLoader)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (tileIdLoader == null)
            {
                throw new ArgumentNullException("tileIdLoader");
            }

            IList<MapLayer> layers = new List<MapLayer>();

            foreach (XElement layer in map.Elements("layer"))
            {
                MapLayer mapLayer = GetLayerMetadata(layer);

                tileIdLoader.LoadLayer(mapLayer, layer.Element("data"));

                layers.Add(mapLayer);
            }

            return layers;
        }

        private static MapLayer GetLayerMetadata(XElement layer)
        {
            return new MapLayer((string) layer.Attribute("name"),
                                (int) layer.Attribute("width"),
                                (int) layer.Attribute("height"));
        }
    }
}
