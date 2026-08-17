using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarboLifeUI
{
    /// <summary>
    /// Makes the native SkiaSharp library that LiveCharts2 depends on loadable when the
    /// 4.8 build is hosted by Revit or Grasshopper.
    ///
    /// On .NET 8 this is a no-op: .NET Core resolves native assets through deps.json and
    /// the runtimes folder, and rolls assembly versions forward. Every call below is
    /// compiled out. It is still safe to call from shared code.
    ///
    /// Safe to call repeatedly and from any entry point; only the first call does work.
    /// </summary>
    public static class NativeDependencies
    {
#if NETFRAMEWORK
        private static bool _done;
        private static readonly object _lock = new object();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);
#endif

        public static void Preload()
        {
#if NETFRAMEWORK
            lock (_lock)
            {
                if (_done)
                    return;
                _done = true;

                try
                {
                    string dir = Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location);

                    if (string.IsNullOrEmpty(dir))
                        return;

                    LoadNativeSkiaSharp(dir);
                    RegisterAssemblyResolver(dir);
                }
                catch
                {
                    //Deliberately swallowed: a failure here should degrade the charts,
                    //not stop the host add-in from loading at all.
                }
            }
#endif
        }

#if NETFRAMEWORK
        /// <summary>
        /// Windows resolves DllImport("libSkiaSharp") against the *process* directory
        /// (Revit's or Rhino's install folder), the system directories and PATH - never
        /// against the folder holding this assembly. So charts fail with "libSkiaSharp
        /// cannot be loaded" even though the file sits right next to CarboLifeUI.dll.
        ///
        /// Loading it by absolute path puts the module in the process table under the name
        /// "libSkiaSharp", so the later DllImport resolves to the already-loaded module.
        /// </summary>
        private static void LoadNativeSkiaSharp(string dir)
        {
            //SkiaSharp's .NET Framework targets populate the architecture subfolders;
            //Directory.Build.targets mirrors the x64 copy into the root. Probe both.
            //IntPtr.Size is enough here - .NET Framework only runs 32- or 64-bit.
            string arch = IntPtr.Size == 8 ? "x64" : "x86";

            string[] candidates =
            {
                Path.Combine(dir, "libSkiaSharp.dll"),
                Path.Combine(dir, arch, "libSkiaSharp.dll"),
            };

            foreach (string nativePath in candidates)
            {
                if (!File.Exists(nativePath))
                    continue;

                if (LoadLibrary(nativePath) != IntPtr.Zero)
                    break;
            }
        }

        /// <summary>
        /// Resolves our NuGet dependencies out of the add-in folder, ignoring the exact
        /// assembly version requested.
        ///
        /// CarboLifeCalc.exe.config carries generated bindingRedirects, so the standalone
        /// application is fine. A Revit add-in is not: the CLR reads Revit.exe.config, so
        /// our redirects never apply. SkiaSharp binds against
        /// System.Runtime.CompilerServices.Unsafe 4.0.4.1 while the folder ships 6.0.0.0,
        /// which surfaces as a FileNotFoundException inside SKObject's type initializer
        /// and looks like "libSkiaSharp cannot be loaded". System.Text.Json brings a
        /// similar set with it.
        ///
        /// Only assemblies that actually sit alongside this one are resolved, so this
        /// cannot interfere with the host's own assembly loading.
        /// </summary>
        private static void RegisterAssemblyResolver(string dir)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    string simpleName = new System.Reflection.AssemblyName(args.Name).Name;

                    //Never try to satisfy satellite/resource lookups - that recurses.
                    if (simpleName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                        return null;

                    string candidate = Path.Combine(dir, simpleName + ".dll");

                    if (File.Exists(candidate))
                        return System.Reflection.Assembly.LoadFrom(candidate);
                }
                catch
                {
                    //Fall through and let the CLR report the original failure.
                }

                return null;
            };
        }
#endif
    }
}
