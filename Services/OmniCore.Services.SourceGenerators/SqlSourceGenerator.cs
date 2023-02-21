using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Omnicore.Services.SourceGenerators
{
    [Generator]
    public class SqlSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("yadayada.g.cs", "namespace zzz { public class ozz { } }");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}