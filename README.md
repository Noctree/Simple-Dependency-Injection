# Simple-Dependency-Injection

A very simple C# dependency injection library.
Originally made for use in the Godot game engine, but it could be used in any other engine, or even in a standalone C# project
(although for that I would highly recommend an actual DI library, as this is very basic)

Supports waiting for dependency registration. For example if Godot node A has node B as dependency, but A called ResolveDependencies before B, it can wait for B to register itself as a dependency and then get notified once all the dependencies are available trough an interface callback.
