using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TmxCSharp.Models;

namespace TmxCSharp.Loader
{
    internal class TileIdLoader
    {
        private readonly TileMapSize _size;
        private readonly int _expectedIds;
        
        public TileIdLoader(TileMapSize tileMapSize)
        {
            if (tileMapSize == null)
            {
                throw new ArgumentNullException("tileMapSize");
            }

            _size = tileMapSize;
            _expectedIds = tileMapSize.Width * tileMapSize.Height;
        }

        public void LoadLayer(MapLayer mapLayer, XElement layerData)
        {
            if (mapLayer == null)
            {
                throw new ArgumentNullException("mapLayer");
            }

            if (layerData == null)
            {
                throw new InvalidDataException("Layer does not have a data element");
            }
            
            string encoding = (string)layerData.Attribute("encoding");

            switch (encoding)
            {
                case "base64":
                    ApplyIds(GetMapIdsFromBase64(layerData.Value, (string)layerData.Attribute("compression")), mapLayer);
                    break;

                case "csv":
                    ApplyIds(ParseCsvData(layerData), mapLayer);
                    break;

                default:
                    if (string.IsNullOrEmpty(encoding))
                    {
                        ApplyIds(GetMapIdsFromXml(layerData.Elements("tile")), mapLayer);
                    }
                    else
                    {
                        throw new InvalidDataException("Unsupported layer data encoding (expected base64 or csv)");
                    }
                    break;
            }
        }

        private IEnumerable<int> GetMapIdsFromBase64(string value, string compression)
        {
            return GetMapIdsFromBytes(Decompression.Decompress(compression, Convert.FromBase64String(value)));
        }

        private IEnumerable<int> GetMapIdsFromXml(IEnumerable<XElement> tiles)
        {
            return tiles.Select(tile => (int) tile.Attribute("gid"));
        }

        private void ApplyIds(IEnumerable<int> ids, MapLayer layer)
        {
            IEnumerator<int> enumerator = ids.GetEnumerator();

            for (int y = 0; y < _size.Height; y++)
            {
                for (int x = 0; x < _size.Width; x++)
                {
                    layer.TileIds[y, x] = enumerator.Current;

                    enumerator.MoveNext();
                }
            }
        }

        private IEnumerable<int> ParseCsvData(XElement layerData)
        {
            return layerData.Value.Split(new[] { ',' }).Select(int.Parse);
        }

        private IEnumerable<int> GetMapIdsFromBytes(byte[] decompressedData)
        {
            int expectedBytes = _expectedIds * 4;

            if (decompressedData.Count() != expectedBytes)
            {
                throw new InvalidDataException("Decompressed data is not identical in size to map");
            }

            for (int tileIndex = 0; tileIndex < expectedBytes; tileIndex += 4)
            {
                yield return GetTileId(decompressedData, tileIndex);
            }
        }

        private static int GetTileId(byte[] decompressedData, int tileIndex)
        {
            const uint flippedHorizontallyFlag = 0x80000000;
            const uint flippedVerticallyFlag = 0x40000000;
            const uint flippedDiagonallyFlag = 0x20000000;
            const uint flipMask = ~(flippedHorizontallyFlag | flippedVerticallyFlag | flippedDiagonallyFlag);

            long tileId = decompressedData[tileIndex]
                          | (decompressedData[tileIndex + 1] << 8)
                          | (decompressedData[tileIndex + 2] << 16)
                          | (decompressedData[tileIndex + 3] << 24);

            // TODO: support these flags

            bool flippedHorizontally = (tileId & flippedHorizontallyFlag) > 0;
            bool flippedVertically = (tileId & flippedVerticallyFlag) > 0;
            bool flippedDiagonally = (tileId & flippedDiagonallyFlag) > 0;

            return (int)(tileId & flipMask);
        }
    }
}
