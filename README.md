This is an unofficial fork of [squirrel.windows](https://github.com/Squirrel/Squirrel.Windows), being [published to nuget](https://www.nuget.org/packages/ppy.squirrel.windows) primarily for consumption by [osu!](https://github.com/ppy/osu). It brings with it some fixes that improve compatibility and performance. It also adds support for .NET core projects.

All changes differing from the official fork are listed below:

- [Update SharpCompress version](https://github.com/Squirrel/Squirrel.Windows/pull/1362)
- [Fix div-by-zero errors on files which don't have any changed content](https://github.com/Squirrel/Squirrel.Windows/pull/1361)
- [Update: Ensure we don't create stub for generated stub executables](https://github.com/Squirrel/Squirrel.Windows/pull/1355)
- [Updated DeltaCompressionDotNet to 2.0.0](https://github.com/Squirrel/Squirrel.Windows/pull/1326) (with [modifications](https://github.com/ppy/Squirrel.Windows/commit/a5a1cb6bc70d11cde28722d9b109339af3497c4f))
- [Don't extract the release package file twic](https://github.com/Squirrel/Squirrel.Windows/pull/1312)
- [Add .NET Framework 4.7.2 support](https://github.com/Squirrel/Squirrel.Windows/pull/1306)
- [Optimization for big packages](https://github.com/Squirrel/Squirrel.Windows/pull/1123)
- [Support for .NET core apps](https://github.com/ppy/Squirrel.Windows/commit/aef207bdcccd47a4b432706c640b2da533f02232) (not yet PR'd as it feels a bit hacky).

Also built against the latest WIndows 10 SDK, which required some [local changes](https://github.com/ppy/Squirrel.Windows/commit/435e784e2f204d9f423b616f16348cda2d51d6b3).

For more information on this project, please check out the [official fork's README](https://github.com/squirrel/squirrel.windows).
