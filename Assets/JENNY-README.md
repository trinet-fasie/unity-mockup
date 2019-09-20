# Jenny

Jenny is a flexible code generation application which you can customize with your own plugins.
Entitas ships with Jenny and several plugins including the `Entitas.Roslyn` plugin which parses
your project and lets you generate code even if there compilation errors.

Unfortunately, the `Entitas.Roslyn` currently cannot be loaded in Unity and can only be used
with the Jenny command line application.

# Quick Start with Jenny and Entitas
- Download and import [Entitas from the Unity Asset Store](https://www.assetstore.unity3d.com/#!/content/87638)
- Open Unity menu item `Tools > Entitas > Preferences...` This will create the initial `Preferences.properties` file in the root of your project folder (outside of the Assets folder)
- Unzip `Jenny.zip` into the root of your project folder (outside the Assets folder)
- Double-click and run
  - `Jenny-Auto-Import` (on Mac and Linux)
  - `Jenny-Auto-Import.bat` (on Windows)
- Alternatively you can run Jenny in the terminal
  - navigate in the root of your project folder (not the Jenny folder) `cd path/to/my/project`
  - run `mono Jenny/Jenny.exe auto-import -s` (on Mac and Linux)
  - run `Jenny\Jenny.exe auto-import -s` (on Windows)
- When prompted "Potential plugin collision" please select "Keep Entitas.Roslyn.[...]" to enable the `Entitas.Roslyn` plugin
- You can now start Jenny as a server
  - double-click `Jenny-Server` (on Mac and Linux)
  - double-click `Jenny-Server.bat` (on Windows)
- Alternatively you can run the server command
  - run `mono Jenny/Jenny.exe server` (on Mac and Linux)
  - run `Jenny\Jenny.exe server` (on Windows)

That's it! You're done!

# Notes

To see all available Jenny commands run Jenny without any arguments
  - run `mono Jenny/Jenny.exe` (on Mac and Linux)
  - run `Jenny\Jenny.exe` (on Windows)

Most common Jenny commands besides `auto-import` and `server` are
- `gen`
- `doctor`
- `fix`

The Entitas Preferences Window `Tools > Entitas > Preferences...` lets you customize Jenny.
You have even more control when you manually edit the `Preferences.properties` file.

# Video Tutorial

See the introduction video on the Desperate Devs YouTube channel
[Entitas ECS Unity Tutorial - Setup & Basics](https://www.youtube.com/watch?v=L-18XRTarOM)
