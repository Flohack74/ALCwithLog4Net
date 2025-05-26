
using System.Diagnostics;
using DevExpress.Pdf.ContentGeneration.Interop;
using System.Reflection;
using System.Runtime.Loader;

namespace HostApp
{
    internal class Program
    {
        static void Main()
        {
            var references = new List<WeakReference>();

            for (int i = 0; i < 10; i++)
            {
                var test = new Worker();
                Console.WriteLine("Hello, World!");
                var reference = test.DoSomeWork();
                if (reference == null)
                {
                    return;
                }

                references.Add(reference);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Thread.Sleep(500);
            }

            foreach (var weakReference in references)
            {
                Console.WriteLine($"Reference alive? {weakReference.IsAlive}");
            }

            Debugger.Launch();
        }
    }

    internal class Worker
    {
        public WeakReference DoSomeWork()
        {
            #if DEBUG
            var flavour = "Debug";
#else
            var flavour = "Release";
#endif
            var pluginPath = Path.GetFullPath(
                string.Concat(Directory.GetCurrentDirectory(), $"\\..\\..\\..\\..\\Plugin\\bin\\{flavour}\\net10.0"));
            var loadContext = new PluginLoadContext(pluginPath);

            //Bootstrapping .net libraries seems to prevent stuck 
            var charpDllLocation = typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location;
            var rootPath = Path.GetDirectoryName(charpDllLocation);
            var assemblies = new List<string>
            {
                //"Microsoft.CSharp.dll",
                //"System.Threading.dll",
                //"System.Private.Xml.dll",
                //"System.Console.dll",
                //"System.Private.Uri.dll",
                //"System.Collections.Specialized.dll",
                //"System.Collections.Concurrent.dll",
                //"System.Configuration.dll"

            };
            foreach (var assembly in assemblies)
            {
                loadContext.LoadFromAssemblyPath(Path.Combine(rootPath, assembly));
            }

            var pluginAssembly = loadContext.LoadFromAssemblyPath($"{pluginPath}\\plugin.dll");
            var pluginType = pluginAssembly.GetType("Plugin.TestPlugin");
            if (pluginType == null)
            {
                Console.WriteLine("Typ not found!");
                return null!;
            }

            // Instanz erzeugen und Methode aufrufen
            object? pluginInstance = Activator.CreateInstance(pluginType);
            MethodInfo? runMethod = pluginType.GetMethod("TestMe");

            if (runMethod != null && pluginInstance != null)
            {
                string result = (string)runMethod.Invoke(pluginInstance, ["Hello from host"])!;
                Console.WriteLine(result);
            }
            runMethod = pluginType.GetMethod("Shutdown");
            runMethod?.Invoke(pluginInstance, []);

            var weakRef = new WeakReference(loadContext, trackResurrection: true);
            loadContext.Unload();
            return weakRef;
        }
    }

    public class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
            Unloading += PluginLoadContext_Unloading;
        }

        private void PluginLoadContext_Unloading(AssemblyLoadContext obj)
        {
            _resolver = null!;
            Unloading -= PluginLoadContext_Unloading;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null!;
        }
    }
}
