using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using TmxCSharp.Models;
using zlib;

namespace TmxCSharp.Loader
{
    /// <summary>
    /// Used to load .tmx files that were created with Tiled.
    /// </summary>
    public static class TmxLoader
    {
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

            TileMapSize tileMapSize = new TileMapSize((int)map.Attribute("width"), (int)map.Attribute("height"), (int)map.Attribute("tilewidth"), (int)map.Attribute("tileheight"));

            IEnumerable<XElement> tileSetDefinitions = map.Elements("tileset");

            IList<TileSet> tileSets = LoadTileSets(tileSetDefinitions);

            IEnumerable<XElement> mapLayerDefinitions = map.Elements("layer");

            IList<MapLayer> layers = new List<MapLayer>();

            foreach (XElement mapLayerDefinition in mapLayerDefinitions)
            {
                MapLayer layer = new MapLayer((string)mapLayerDefinition.Attribute("name"), (int)mapLayerDefinition.Attribute("width"), (int)mapLayerDefinition.Attribute("height"));

                int[] tileIds = ExtractTileIds(mapLayerDefinition, tileMapSize.Width * tileMapSize.Height);

                AddTileIdsToLayer(tileIds, tileMapSize, layer);

                layers.Add(layer);
            }

            return new TileMap(tileMapSize, tileSets, layers);
        }

        private static void AddTileIdsToLayer(int[] tileIds, TileMapSize tileMapSize, MapLayer layer)
        {
            for (int y = 0; y < tileMapSize.Height; y++)
            {
                for (int x = 0; x < tileMapSize.Width; x++)
                {
                    int tileId = tileIds[(y * tileMapSize.Height) + x];

                    layer.TileIds[y, x] = tileId;
                }
            }
        }

        private static int[] ExtractTileIds(XElement mapLayerDefinition, int expectedTileIds)
        {
            XElement layerData = mapLayerDefinition.Element("data");

            if (layerData == null)
            {
                throw new InvalidDataException("Layer does not have a data element");
            }

            string encoding = (string) layerData.Attribute("encoding");

            switch (encoding)
            {
                case "base64":
                    string compression = (string) layerData.Attribute("compression");

                    byte[] compressedData = Convert.FromBase64String(layerData.Value);

                    byte[] decompressedData;

                    switch (compression)
                    {
                        case "zlib":
                            DecompressZLibData(compressedData, out decompressedData);

                            return GetMapIdsFromBytes(expectedTileIds, decompressedData);
                        case "gzip":
                            DecompressGZipData(compressedData, out decompressedData);

                            return GetMapIdsFromBytes(expectedTileIds, decompressedData);
                        default:
                            if (string.IsNullOrEmpty(compression))
                            {
                                return GetMapIdsFromBytes(expectedTileIds, compressedData);
                            }
                            
                            throw new InvalidDataException("Unsupported compression (expected zlib or gzip)");
                    }

                case "csv":
                    return ParseCsvData(expectedTileIds, layerData);
                default:
                    if (string.IsNullOrEmpty(encoding))
                    {
                        return GetMapIdsFromXml(expectedTileIds, layerData.Elements("tile"));
                    }

                    throw new InvalidDataException("Unsupported layer data encoding (expected base64 or csv)");
            }
        }

        private static int[] GetMapIdsFromXml(int expectedMapIds, IEnumerable<XElement> tiles)
        {
            int[] tileIds = new int[expectedMapIds];

            int i = 0;

            foreach (XElement tile in tiles)
            {
                if (i >= expectedMapIds)
                {
                    throw new InvalidDataException("XML data is not identical in size to map");
                }

                tileIds[i] = (int)tile.Attribute("gid");

                i++;
            }

            if (i < expectedMapIds)
            {
                throw new InvalidDataException("XML data is not identical in size to map");
            }

            return tileIds;
        }

        private static int[] ParseCsvData(int expectedMapIds, XElement layerData)
        {
            string[] parsedTileIds = layerData.Value.Split(new[] {','});

            int length = parsedTileIds.Count();
            
            if (length != expectedMapIds)
            {
                throw new InvalidDataException("CSV data is not identical in size to map");
            }

            int[] tileIds = new int[length];

            for (int i = 0; i < length; i++)
            {
                tileIds[i] = int.Parse(parsedTileIds[i]);
            }

            return tileIds;
        }

        private static int[] GetMapIdsFromBytes(int expectedMapIds, byte[] decompressedData)
        {
            int expectedBytes = expectedMapIds*4;

            if (decompressedData.Count() != expectedBytes)
            {
                throw new InvalidDataException("Decompressed data is not identical in size to map");
            }

            int[] tileIds = new int[expectedMapIds];

            for (int tileIndex = 0; tileIndex < expectedBytes; tileIndex += 4)
            {
                tileIds[tileIndex/4] = GetTileId(decompressedData, tileIndex);
            }

            return tileIds;
        }

        private static int GetTileId(byte[] decompressedData, int tileIndex)
        {
            const uint FlippedHorizontallyFlag = 0x80000000;
            const uint FlippedVerticallyFlag = 0x40000000;
            const uint FlippedDiagonallyFlag = 0x20000000;
            const uint FlipMask = ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag);

            long tileId = decompressedData[tileIndex]
                          | (decompressedData[tileIndex + 1] << 8)
                          | (decompressedData[tileIndex + 2] << 16)
                          | (decompressedData[tileIndex + 3] << 24);

            bool flippedHorizontally = (tileId & FlippedHorizontallyFlag) > 0;
            bool flippedVertically = (tileId & FlippedVerticallyFlag) > 0;
            bool flippedDiagonally = (tileId & FlippedDiagonallyFlag) > 0;

            // TODO: support these flags

            return (int)(tileId & FlipMask);
        }

        private static void DecompressZLibData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
                {
                    using (Stream inMemoryStream = new MemoryStream(inData))
                    {
                        CopyStream(inMemoryStream, outZStream);
                        outZStream.finish();
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }

        private static void DecompressGZipData(byte[] inData, out byte[] outData)
        {
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                using (GZipStream outGStream = new GZipStream(inMemoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outMemoryStream = new MemoryStream())
                    {
                        CopyStream(outGStream, outMemoryStream);
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            
            int len;
            
            while ((len = input.Read(buffer, 0, 4096)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            
            output.Flush();
        }

        private static IList<TileSet> LoadTileSets(IEnumerable<XElement> tileSetDefinitions)
        {
            IList<TileSet> tileSets = new List<TileSet>();

            foreach (XElement tileSetDefinition in tileSetDefinitions)
            {
                XElement image = tileSetDefinition.Element("image");

                if (image == null)
                {
                    throw new InvalidDataException("Tile set missing image");
                }

                TileSetImage tileSetImage = new TileSetImage((string) image.Attribute("source"),
                                                             (int) image.Attribute("width"), (int) image.Attribute("height"));

                TileSet tileSet = new TileSet((int) tileSetDefinition.Attribute("firstgid"),
                                              (string) tileSetDefinition.Attribute("name"),
                                              (int) tileSetDefinition.Attribute("tilewidth"),
                                              (int) tileSetDefinition.Attribute("tileheight"), tileSetImage);

                tileSets.Add(tileSet);
            }

            return tileSets;
        }

        private static void AssertRequirements(XElement map)
        {
            string version = (string) map.Attribute("version");

            if (version != "1.0")
            {
                throw new InvalidDataException(string.Format("Unsupported map version '{0}'. Only version 1.0 is supported.",
                                                             version));
            }

            string orientation = (string) map.Attribute("orientation");

            if (orientation != "orthogonal")
            {
                throw new InvalidDataException(
                    string.Format("Unsupported orientation '{0}'. Only orthogonal orientation is supported.", orientation));
            }
        }
    }
}