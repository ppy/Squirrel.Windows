# Squirrel.Windows (DragonFruit Fork)

## Overview

This is a fork of [ppy's fork of squirrel.windows](https://github.com/ppy/Squirrel.Windows)

## Changes

All changes differing from the official fork are listed below:

### From ppy

- [Update SharpCompress version](https://github.com/Squirrel/Squirrel.Windows/pull/1362)
- [Fix div-by-zero errors on files which don't have any changed content](https://github.com/Squirrel/Squirrel.Windows/pull/1361)
- [Update: Ensure we don't create stub for generated stub executables](https://github.com/Squirrel/Squirrel.Windows/pull/1355)
- [Updated DeltaCompressionDotNet to 2.0.0](https://github.com/Squirrel/Squirrel.Windows/pull/1326) (with [modifications](https://github.com/dragonfruitnetwork/Squirrel.Windows/commit/a5a1cb6bc70d11cde28722d9b109339af3497c4f))
- [Don't extract the release package file twic](https://github.com/Squirrel/Squirrel.Windows/pull/1312)
- [Add .NET Framework 4.7.2 support](https://github.com/Squirrel/Squirrel.Windows/pull/1306)
- [Optimization for big packages](https://github.com/Squirrel/Squirrel.Windows/pull/1123)
- [Support for .NET Core apps](https://github.com/dragonfruitnetwork/Squirrel.Windows/commit/aef207bdcccd47a4b432706c640b2da533f02232)

### From DragonFruit

- [Update Cecil to 0.11.2](https://github.com/dragonfruitnetwork/Squirrel.Windows/pulls/1)

## Notes

- This is also built against the latest Windows 10 SDK
- More information can be found from the [official README](https://github.com/squirrel/squirrel.windows)
