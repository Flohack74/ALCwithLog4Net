This is a simple example of how an assembly load context holds on to various assemblies even when it should be fine for unloading. How to reproduce:

- Download the solution and build
- Publish the code to a subfolder in bin\ using the included publish profile
- Start the HostApp.exe from the memory profiler of your choice or from within Visual Studio´s diagnostics
- The app will execute creation and destruction of a plugin 10 times in a loop. In every loop it tries its best to get rid of the ALC
- After the loop HostApp will try to enter the debugger. At this point you should inspect loaded object/types and assemblies still in memory
- Look for objects being in memory exactly 10 times. They are a strong indicator that they were created but not released in the loop of HostApp

Currently my result is the following:

Type, Objects, Bytes, Minimum retained bytes
System.Diagnostics.Tracing.EventSource+OverrideEventProvider, 10, 640, 640
System.Func<Object, PropertyValue>, 10, 640, 640
System.Reflection.LoaderAllocator, 10, 480, 2160
System.Reflection.LoaderAllocatorScout, 10, 240, 240
log4net.Core.LogImpl, 10, 640, 640
log4net.Repository.LoggerRepositoryConfigurationChangedEventHandler, 11, 704, 1496
HostApp.PluginLoadContext, 10, 880, 1120
System.Diagnostics.Tracing.ScalarTypeInfo, 10, 640, 970
System.WeakReference, 10, 240, 240
System.WeakReference<EventProvider>, 10, 240, 240
