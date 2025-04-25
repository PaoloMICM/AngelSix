using Microsoft.Win32;
using SolidWorksTools;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;

namespace Icm.AngelSix.SolidWorksApi.AddinInstaller.Services
{
    public class AddinInstallerService
    {
        #region Fields
        private readonly string _dllPath;
        private readonly Assembly _assemblyToRegister;
        private readonly PluginLoadContext _pluginLoader;
        private readonly AssemblyName[] _references;
        private readonly HashSet<string> _exploredAssemblies = new HashSet<string>();
        #endregion Fields

        #region Constructors

        public AddinInstallerService(string dllPath)
        {
            // Check Dll
            if (string.IsNullOrEmpty(dllPath))
            {
                throw new ArgumentNullException(nameof(dllPath));
            }
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("The specified DLL does not exist.", dllPath);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            this._dllPath = dllPath;
            this._assemblyToRegister = Assembly.LoadFile(dllPath);
            this._pluginLoader = new PluginLoadContext(dllPath);
            this._references = _assemblyToRegister.GetReferencedAssemblies();
        }

        #endregion Constructors

        #region Methods

        public int Register(out StreamReader stdError)
        {
            var isNetCore = true;
            //.NET Framework dlls references mscorlib.dll
            foreach (var reference in _references)
            {
                if (reference.Name == "mscorlib")
                {
                    Console.WriteLine("The DLL is a .NET Framework DLL.");
                    isNetCore = false;
                    break;
                }
            }

            //if (isNetCore)
            //{
            //    var dllPathWoExt = _dllPath.Substring(0, _dllPath.LastIndexOf("."));

            //    //Use regsvr32 on *comhost.dll related to the assembly

            //    //Process.Start("regsvr32", "/s " + dllPathWoExt + ".comhost.dll");
            //    Process.Start("regsvr32", "\"" + dllPathWoExt + ".comhost.dll" + "\"");
            //    RegisterCustomInfo();
            //}
            //else
            //{
            //    Process.Start("regasm", "/codebase " + "\"" + _dllPath + "\"");
            //    RegisterCustomInfo();
            //}


            if (isNetCore)
            {
                var dllPathWoExt = _dllPath.Substring(0, _dllPath.LastIndexOf("."));

                ////Use regsvr32 on *comhost.dll related to the assembly
                //Process.Start("regsvr32", "\"" + dllPathWoExt + ".comhost.dll" + "\"");
                //RegisterCustomInfo();

                // Run the RegAsm with the Dll path as an argument
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "regsvr32",
                        Arguments = $"\"{dllPathWoExt}.comhost.dll\"",
                        // Run as admin
                        Verb = "runas",
                        // Redirect input and output
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Read the output
                stdError = process.StandardError;
                process.WaitForExit();

                // If it exit successfully
                if (process.ExitCode == 0)
                {
                    RegisterCustomInfo();
                }
                // Otherwise just show the results


                return process.ExitCode;
            }
            else
            {
                //Process.Start("regasm", "/codebase " + "\"" + dllPath + "\"");
                //RegisterCustomInfo();

                // Run the RegAsm with the Dll path as an argument
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "regasm",
                        Arguments = $"/codebase \"{_dllPath}\"",
                        // Run as admin
                        Verb = "runas",
                        // Redirect input and output
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Read the output
                stdError = process.StandardError;
                process.WaitForExit();

                // If it exit successfully
                if (process.ExitCode == 0)
                {
                    RegisterCustomInfo();
                }
                // Otherwise just show the results
                

                return process.ExitCode;
            }

        }

        public int UnRegister(out StreamReader stdError)
        {
            var isNetCore = true;
            //.NET Framework dlls references mscorlib.dll
            foreach (var reference in _references)
            {
                if (reference.Name == "mscorlib")
                {
                    Console.WriteLine("The DLL is a .NET Framework DLL.");
                    isNetCore = false;
                    break;
                }
            }

            if (isNetCore)
            {
                var dllPathWoExt = _dllPath.Substring(0, _dllPath.LastIndexOf("."));

                //Process.Start("regsvr32", "/u " + "\"" + dllPathWoExt + ".comhost.dll" + "\"");
                //UnregisterCustomInfo();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "regsvr32",
                        Arguments = $"/u \"{dllPathWoExt}.comhost.dll\"",
                        // Run as admin
                        Verb = "runas",
                        // Redirect input and output
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Read the output
                stdError = process.StandardError;
                process.WaitForExit();

                // If it exit successfully
                if (process.ExitCode == 0)
                {
                    UnregisterCustomInfo();
                }
                // Otherwise just show the results


                return process.ExitCode;
            }
            else
            {
                //Process.Start("regasm", "/unregister " + "\"" + _dllPath + "\"");
                //UnregisterCustomInfo();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "regasm",
                        Arguments = $"/unregister \"{_dllPath}\"",
                        // Run as admin
                        Verb = "runas",
                        // Redirect input and output
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Read the output
                stdError = process.StandardError;
                process.WaitForExit();

                // If it exit successfully
                if (process.ExitCode == 0)
                {
                    UnregisterCustomInfo();
                }
                // Otherwise just show the results


                return process.ExitCode;
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();
                return types;
            }
            catch (ReflectionTypeLoadException e)
            {
                var types = e.Types.Where(t => t != null).ToList();
                return types;
                //return null;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Attempting to resolve assembly: " + args.Name);

            // Check if the assembly has already been explored
            if (_exploredAssemblies.Contains(args.Name))
            {
                Console.WriteLine("Assembly already explored: " + args.Name);
                return null;
            }

            // Add the assembly name to the explored set
            _exploredAssemblies.Add(args.Name);

            var assyName = new AssemblyName(args.Name);

            // Inform the resolver to use the path of the assembly to register
            var retVal = _pluginLoader.LoadFromAssemblyName(assyName);
            if (retVal != null)
            {
                return retVal;
            }
            else
            {
                Console.WriteLine("Assembly not found: " + args.Name);
                return null;
            }

            //Console.WriteLine("Attempting to resolve assembly: " + args.Name);
            //// Implement logic to locate and load the missing assembly
            //// For example, you can return Assembly.LoadFrom(pathToAssembly);
            //var assyName = new AssemblyName(args.Name);

            ////Inform the resolver to use the path of the assembly to register

            //var retVal = _pluginLoader.LoadFromAssemblyName(assyName);
            //if (retVal != null)
            //{
            //    return retVal;
            //}
            //else
            //    throw new Exception("Assembly not found: " + args.Name);
        }

        private void RegisterCustomInfo()
        {
            foreach (Type t in GetLoadableTypes(_assemblyToRegister))
            {
                foreach (System.Attribute attr in t.GetCustomAttributes(false))
                {
                    if (attr is SwAddinAttribute swAttr)
                    {
                        try
                        {
                            RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                            RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                            string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                            RegistryKey addinkey = hklm.CreateSubKey(keyname);
                            addinkey.SetValue(null, 0);

                            addinkey.SetValue("Description", swAttr.Description);
                            addinkey.SetValue("Title", swAttr.Title);

                            keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                            addinkey = hkcu.CreateSubKey(keyname);
                            addinkey.SetValue(null, Convert.ToInt32(swAttr.LoadAtStartup), Microsoft.Win32.RegistryValueKind.DWord);

                            Console.WriteLine("The dll has been registered successfully.");
                        }
                        catch (System.NullReferenceException nl)
                        {
                            Console.WriteLine("There was a problem registering this dll: SWattr is null. \n\"" + nl.Message + "\"");
                            Console.WriteLine("There was a problem registering this dll: SWattr is null.\n\"" + nl.Message + "\"");
                        }
                        catch (System.Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("There was a problem registering the function: \n\"" + e.Message + "\"");
                        }

                        break;
                    }
                    //else if (attr is ComUnregisterFunctionAttribute)
                    //{
                    //    if (operation.ToLower() == "unregister")
                    //    {
                    //        m.Invoke(null, null);
                    //    }
                    //}
                }
            }
        }

        private void UnregisterCustomInfo()
        {
            foreach (Type t in GetLoadableTypes(_assemblyToRegister))
            {
                foreach (System.Attribute attr in t.GetCustomAttributes(false))
                {
                    if (attr is SwAddinAttribute swAttr)
                    {
                        try
                        {
                            RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                            RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                            string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                            hklm.DeleteSubKey(keyname);

                            keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                            hkcu.DeleteSubKey(keyname);

                            Console.WriteLine("The dll has been unregistered successfully.");
                        }
                        catch (System.NullReferenceException nl)
                        {
                            Console.WriteLine("There was a problem unregistering this dll: " + nl.Message);
                            Console.WriteLine("There was a problem unregistering this dll: \n\"" + nl.Message + "\"");
                        }
                        catch (System.Exception e)
                        {
                            Console.WriteLine("There was a problem unregistering this dll: " + e.Message);
                            Console.WriteLine("There was a problem unregistering this dll: \n\"" + e.Message + "\"");
                        }
                    }
                }
            }
        }

        #endregion Methods
    }

    internal class PluginLoadContext : AssemblyLoadContext
    {
        #region Fields
        private AssemblyDependencyResolver _resolver;
        #endregion Fields

        #region Constructors

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        #endregion Constructors

        #region Methods

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        #endregion Methods
    }
}