using System;
using System.IO;
using System.Reflection;

namespace kagami
{
    public static class AssemblyResolver
    {
        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly tryLoadAssembly(
                string directory,
                string extension)
            {
                var asm = new AssemblyName(args.Name);

                var asmPath = Path.Combine(directory, asm.Name + extension);
                if (File.Exists(asmPath))
                {
                    return Assembly.LoadFrom(asmPath);
                }

                return null;
            }

            var dir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "bin");

            foreach (var directory in new[] { dir })
            {
                var asm = tryLoadAssembly(directory, ".dll");
                if (asm != null)
                {
                    return asm;
                }
            }

            return null;
        }
    }
}
