using System;
using System.IO;
using System.Reflection;

using Rhino.Runtime.InProcess;
using Rhino.Geometry;
using Rhino;

namespace HelloWorld
{
  class Program
  {
    #region Program static constructor
    static Program()
    {
      ResolveEventHandler OnRhinoCommonResolve = null;
      AppDomain.CurrentDomain.AssemblyResolve += OnRhinoCommonResolve = (sender, args) =>
      {
        const string rhinoCommonAssemblyName = "RhinoCommon";
        var assemblyName = new AssemblyName(args.Name).Name;

        if (assemblyName != rhinoCommonAssemblyName)
          return null;

        AppDomain.CurrentDomain.AssemblyResolve -= OnRhinoCommonResolve;

        string rhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
        return Assembly.LoadFrom(Path.Combine(rhinoSystemDir, rhinoCommonAssemblyName + ".dll"));
      };
    }
    #endregion

    [System.STAThread]
    static void Main(string[] args)
    {
      try
      {

        using (new RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden ))
        {

          Console.WriteLine("Enter path to directory which contains files to convert and press ENTER:");
          string path = Console.ReadLine();

          String[] filePaths = null;

          try
          {
            filePaths = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
          }
          catch (Exception e)
          {
            Console.WriteLine("{0}, Press any key to exit...",e.Message);
            Console.ReadKey();
            return;
          }

          if (filePaths == null || filePaths.Length == 0)
          {
            Console.WriteLine("Directory is empty. No files to process. Press any key to exit.");
            Console.ReadKey();
            return;
          }

          var doc = RhinoDoc.ActiveDoc;
          
          foreach (string file in filePaths)
          {

            var script = string.Format("_-Import \"{0}\" _Enter", file);
            RhinoApp.RunScript(script, false);

            // Change the original extension to .obj
            var objPath = System.IO.Path.ChangeExtension(file, ".obj");

            // Export options:

            var fowo = new Rhino.FileIO.FileObjWriteOptions(new Rhino.FileIO.FileWriteOptions())
            {
              ExportMaterialDefinitions = false,
              MapZtoY = true
            };

            // Save the .obj

            Console.WriteLine("Nº of objects in file: {0}", doc.Objects.Count);

            var result = Rhino.FileIO.FileObj.Write(objPath, doc, fowo);

            if(result == Rhino.PlugIns.WriteFileResult.Success)
              Console.WriteLine("Converted file: {0}", objPath);
            else
              Console.WriteLine("File conversion failed.");

            // Delete all of the imported objects before importing a new file

            foreach (Rhino.DocObjects.RhinoObject ro in doc.Objects)
              doc.Objects.Delete(ro);

          }

          Console.WriteLine("Finished converting. Press any key to exit...");
          Console.ReadKey();
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex.Message);
        Console.WriteLine("press any key to exit");
        Console.ReadKey();
      }
    }
  }
}
