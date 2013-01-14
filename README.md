# TMXCSharp

A TMX Map parser for Tiled map engine written in C#.

## Goal of library:

* Easy to use
* Use Classes to eliminate the possibility of accidental struct copies
* Reasonably fast

## Potential Uses:

* Orthographic (2D looking straight-on) Tile Engines in C#
* Easy to integrate with OpenTK or XNA

## Runtime Requirements:

* .net 4, .net 4 client profile, or .net 4.5 (default)

## License:

* MIT (see LICENSE.md)

## Getting Started:

	// First parameter is the tmx file output by Tiled.
	// Second parameter is the location where all your tile set images are stored
	// NOTE: second parameter allows you to have images in a different location in your application relative to the map file than when you were editing
	// NOTE: This is currently required but will be optional in the future to use the relative path in the map file
	var tileMap = TmxLoader.Parse(@"Assets\Maps\mymap.tmx", @"Assets\Images\TileSets");

## My OpenTK Tile Renderer:

	// This is straight out of a game I'm working on.
	// I used it to verify this library is working correctly.
	// You would obviously need OpenTK and the write your own TextureLoader
	
    internal class MapRenderer
    {
        public MapRenderer(TileMap map)
        {
            Map = map;
        }

        public TileMap Map { get; private set; }

        public int X { get; set; }

        public int Y { get; set; }

        public void Render(int width, int height)
        {
            int tileHeight = Map.Size.TileHeight;
            int tileWidth = Map.Size.TileWidth;

            int widthInTiles = (int)Math.Ceiling((double)width/tileWidth);
            int heightInTiles = (int)Math.Ceiling((double)height/tileHeight);

            foreach (MapLayer layer in Map.Layers)
            {
                for (int offsetY = Y; offsetY <= Y + heightInTiles; offsetY++)
                {
                    for (int offsetX = X; offsetX <= X + widthInTiles; offsetX++)
                    {
                        if (offsetX >= 0 && offsetY >= 0 && offsetX < Map.Size.Width && offsetY < Map.Size.Height)
                        {
                            int tileId;
                            unchecked
                            {
                                tileId = layer.TileIds[offsetY, offsetX];
                            }

                            if (tileId > 0)
                            {
                                DisplayTileId(tileId, offsetY*tileHeight, offsetX*tileWidth,
                                              tileHeight, tileWidth);
                            }
                        }
                    }
                }
            }
        }

        private void DisplayTileId(int id, int y, int x, int tileHeight, int tileWidth)
        {
            TileSet set = GetTileSet(id);

            string imageFile = set.Image.FilePath;

            int tileSetHeight = set.Image.Height;
            int tileSetWidth = set.Image.Width;

            int texture = GetTexture(imageFile);

            int tileOffset = id - set.FirstId;
            int textureOffsetX = tileOffset % (tileSetHeight / tileHeight);
            int textureOffsetY = tileOffset / (tileSetHeight / tileHeight);

            float textureX = (float)(textureOffsetX * tileWidth) / tileSetWidth;
            float textureY = (float)(textureOffsetY * tileHeight) / tileSetHeight;

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(textureX, textureY); GL.Vertex2(x, y);
            GL.TexCoord2(textureX, textureY + ((float)tileHeight / tileSetHeight)); GL.Vertex2(x, y + tileHeight);
            GL.TexCoord2(textureX + ((float)tileWidth / tileSetWidth), textureY + ((float)tileHeight / tileSetHeight)); GL.Vertex2(x + tileWidth, y + tileHeight);
            GL.TexCoord2(textureX + ((float)tileWidth / tileSetWidth), textureY); GL.Vertex2(x + tileWidth, y);

            GL.End();
        }

        private int GetTexture(string imageFile)
        {
            return TextureLoader.Load(imageFile);
        }

        private TileSet GetTileSet(int tileId)
        {
            return Map.TileSets.First(tileSet => tileSet.ContainsTile(tileId));
        }
    }
