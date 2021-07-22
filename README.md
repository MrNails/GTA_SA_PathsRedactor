# GTA_SA_PathsRedactor
Just a simple paths editor for Grand Theft Auto: San Andreas.

# Path editing
This program have toolbox for editing tram\train paths.
Also it have ability to save path changes during work with path.
You can download this program <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/releases/tag/App_1.0.7873.21459">there<a/>.

# Custom path loader and saver
You can create your own path loader and saver using C#.
First of all - you need download <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/releases/tag/DLL_1.0.7873.21459">GTA_SA_PathRedactor.dll<a/>. Then, you add this DLL to yout project and inherit abstract classes <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/blob/App_1.0.7873.21459/GTA_SA_PathsRedactor.Utilities/PointSaver.cs">Saver<a/> and <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/blob/App_1.0.7873.21459/GTA_SA_PathsRedactor.Utilities/PointLoader.cs">Loader<a/> (see an <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/tree/App_1.0.7873.21459/GTA_SA_PathsReadactor.Test">example<a/>). Also you can create <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/blob/App_1.0.7873.21459/GTA_SA_PathsRedactor/Settings/DefaultInfo.json">JSON<a/> file with information about your savers and loaders. But this JSON file must be named as your DLLs name but with .json extension. This file have such properpties:
  * Name - full name of your type (Can be recieved use GetType() method or keyword typeof(YourType);
  * Purpose - just purpose of your saver/loader;
  * Description - just description of your saver/loader;
When you created your own classes (or class, not necessary create both loader and saver) you may use it in program.

To use they in the program, you need open selection window in menu <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/blob/master/Examples/Pictures/PointSaverAndLoaderMenuItem.png">Settings->Loaders and savers information<a/> and <a href="https://github.com/MrNails/GTA_SA_PathsRedactor/blob/master/Examples/Pictures/LoadCustomDLLButton.png">Settings->LOad<a/> your DLL. Then you need press <b>Select<b/> button and you can use your saver/loader.
  
For help about hotkey you can open Help window in menu item <b>Windows<b/> or press F1.
