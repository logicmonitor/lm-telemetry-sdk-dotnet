using System;
using System.Runtime.Loader;
using System.IO;
using System.Reflection;
/// <summary>
/// Dotnet StartupHook
/// </summary>
internal class StartupHook
{
    /// <summary>
    /// Load and initialize OpenTelemetry Startup with a pre-defined set of environment variables for instrumentations.
    /// </summary>
    public static void Initialize()
    {
            try
            {
            //Load Assemblies required for resource detector.
            var _disableResourceDetection = Environment.GetEnvironmentVariable("DISABLE_RESOURCE_DETECTION") ?? "0";
            var loaderAssembly = Environment.GetEnvironmentVariable("DOTNET_STARTUP_HOOKS");
            var loaderdirectory = Path.GetDirectoryName(loaderAssembly);
            var context = new AssemblyLoadContext("LMStartupHook", true);

            if (_disableResourceDetection.Equals("0"))
            {
                var LMloaderDirectory = Path.Combine(loaderdirectory, "LMTelemetry");
                DirectoryInfo d = new DirectoryInfo(@LMloaderDirectory); 
                FileInfo[] Files = d.GetFiles("*.dll");
                foreach (FileInfo file in Files)
                {
                    context.LoadFromAssemblyPath(Path.Combine(LMloaderDirectory,file.Name));
                    //Console.WriteLine("Loaded Assembly:"+file.Name);
                }
                Environment.SetEnvironmentVariable("SET_RESOURCE_ATTRIBUTE", "1");
                Console.WriteLine("Initalising Resource");
                Assembly LMResourceDetector = context.LoadFromAssemblyPath(Path.Combine(LMloaderDirectory, "LMTelemetrySDK" + ".dll"));
                InvokerResourceDetectMethod(LMResourceDetector, context);
            }
            else{
                Console.WriteLine("LM Resource Detector Disabled!");
            }
            InvokerOtelStartupHookMethod(loaderdirectory, context);
            Console.WriteLine("LMStartupHook initialized successfully!");
            }
            catch (Exception ex)
            {
            Console.WriteLine($"LMStartupHook initialize error! : {ex}");
                throw;
            }

    }
    /// <summary>
    /// Invoke Detect method of LMResourceDetector to detect resource attributes.
    /// </summary>
    private static void InvokerResourceDetectMethod(Assembly loaderAssembly,AssemblyLoadContext context)
    {
        //Console.WriteLine($"Get LMResourceDetector.main type");
        var loaderType = loaderAssembly.GetType("LMResourceDetector.ResourceDetector");

        if (loaderType is null)
        {
            Console.WriteLine($"LMResourceDetector.ResourceDetector type is null");
            return;
        }

        //Console.WriteLine($"Get LMResourceDetector.main.Detect method");
        var initializeMethod = loaderType.GetMethod("Detect", BindingFlags.Public | BindingFlags.Static);

        if (initializeMethod is null)
        {
            Console.WriteLine($"Detect method is null");
            return;
        }

        //Console.WriteLine($"Invoke Detect method");
        initializeMethod.Invoke(null, null);
    }

    private static void InvokerOtelStartupHookMethod(String loaderdirectory , AssemblyLoadContext context)
    {
        //Console.WriteLine($"Get OpenTelemetry.Instrumentation.StartupHook type");
        var otelStartHookpath = Path.Combine(loaderdirectory, "OpenTelemetry.AutoInstrumentation.StartupHook" + ".dll");
        Assembly OtelHook = context.LoadFromAssemblyPath(otelStartHookpath);
        var loaderType = OtelHook.GetType("StartupHook");

        if (loaderType is null)
        {
            Console.WriteLine($"StartupHook type is null");
            return;
        }

        //Console.WriteLine($"Get StartupHook.Initialize method");
        var initializeMethod = loaderType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

        if (initializeMethod is null)
        {
            Console.WriteLine($"Initialize method is null");
            return;
        }

        //Console.WriteLine($"Invoke Initialize method");
        initializeMethod.Invoke(null, null);
    }
}

