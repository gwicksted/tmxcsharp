# TMXCSharp

A TMX Map parser for Tiled map engine written in C#.

## Goal of Library:

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

## Features Supported:

* Multiple Tile Layers
* Multiple Tile Sets
* Sanity checks on data
* Base64 + zlib/gzip/or no compression
* CSV format
* XML format
* Method for providing a different location for the tile images (instead of the one the map editor was given)

## Features Not Yet Supported:

* Properties
* Isometric (I have only attempted orthogonal)
* Object layers
* Flags on tiles.  They get dropped and the tile id is corrected.  Horizontal, vertical, and diagonal flipping is parsed but no indication is given on the output model.
* Using the path that the map editor gave for each tileset instead of different location provided.
* Needs commenting and refactoring of the code.
* Persisting map data back to a .tmx file

## External Links:

Tiled github page - https://github.com/bjorn/tiled

Tiled home page - http://www.mapeditor.org/

List of supported parsers - https://github.com/bjorn/tiled/wiki/Support-for-TMX-maps

## Getting Started:

SORRY! This section is undergoing a rewrite due to recent refactoring.  A new open source tile engine will be released soon including support for TmxCSharp.

That said, the library is pretty straight-forward. Look here first:

https://github.com/gwicksted/tmxcsharp/blob/master/TmxCSharp/Loader/TmxLoader.cs
