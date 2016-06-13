using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace repro
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = LoadIntoAppDomain("E:/repro/minrepro/src/hello.cs");
            assembly.GetTypes().First(t => t.FullName == "src.Test").GetMethod("Hello").Invoke(null, null);
        }

        public static Assembly LoadIntoAppDomain(string srcfile, bool debug = true)
        {
            string assemblyName = Path.GetRandomFileName();
            string assemblyDll = Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".dll");
            string assemblyPdb = Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".pdb");

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new List<SyntaxTree> { CSharpSyntaxTree.ParseText(File.ReadAllText(srcfile)) },
                references: AppDomain.CurrentDomain.GetAssemblies().Select(a => MetadataReference.CreateFromFile(a.Location)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = File.OpenWrite(assemblyDll))
            using (var pdb_ms = File.OpenWrite(assemblyPdb))
            {
                EmitResult result = compilation.Emit(ms, pdb_ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    throw new Exception();
                }
            }
            return Assembly.Load(File.ReadAllBytes(assemblyDll), File.ReadAllBytes(assemblyPdb));
        }
    }
}

