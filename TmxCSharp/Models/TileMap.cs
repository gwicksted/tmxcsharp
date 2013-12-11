using System;
using System.Collections.Generic;

namespace TmxCSharp.Models
{
    public class TileMap
    {
        public TileMap(TileMapSize size, IList<TileSet> tileSets, IList<MapLayer> layers)
        {
            if (size == null)
            {
                throw new ArgumentNullException("size");
            }

            if (tileSets == null)
            {
                throw new ArgumentNullException("tileSets");
            }

            if (layers == null)
            {
                throw new ArgumentNullException("layers");
            }

            Size = size;

            TileSets = tileSets;

            Layers = layers;
        }

        public TileMapSize Size { get; private set; }

        public IList<TileSet> TileSets { get; private set; }

        public IList<MapLayer> Layers { get; private set; }
    }
}