
using System.Diagnostics;
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

            //Ideally the weak references should be dead at this point
            foreach (var weakReference in references)
            {
                Console.WriteLine($"Reference alive? {weakReference.IsAlive}");
            }

            //Check here with profiler if the plugin and/or log4net assembly is still loaded
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
            var loadContext = new PluginLoadContext(Path.Combine(Directory.GetCurrentDirectory(), "HostApp.dll"));

            var pluginAssembly = loadContext.LoadFromAssemblyPath($"{Directory.GetCurrentDirectory()}\\plugin.dll");
            var pluginType = pluginAssembly.GetType("Plugin.TestPlugin");
            if (pluginType == null)
            {
                Console.WriteLine("Typ not found!");
                return null!;
            }

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
            var assembly = base.Load(assemblyName);
            if (assembly != null)
            {
                return assembly;
            }
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null!;
        }
    }
}
