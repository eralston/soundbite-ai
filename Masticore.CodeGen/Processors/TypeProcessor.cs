using Masticore.CodeGen.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Masticore.CodeGen.Processors
{
    public class TypeProcessor
    {
        public void Process(CodeGenContext context, XmlDocHelper xmlDocHelper)
        {
            ControllerProcessor controllerProcessor = new ControllerProcessor(context, xmlDocHelper);
            EnumProcessor enumProcessor = new EnumProcessor(context, xmlDocHelper);
            ModelProcessor modelProcessor = new ModelProcessor(context, xmlDocHelper);

            System.Collections.Generic.IEnumerable<Type> nextBatch = context.GetNextBatch();
            while (nextBatch?.Any() == true)
            {
                nextBatch.ForEach(typeToProcess =>
                {
                    Console.WriteLine("Processing: {0}", typeToProcess.Name);
                    if (typeof(ControllerBase).IsAssignableFrom(typeToProcess))
                    {
                        controllerProcessor.ProcessType(typeToProcess);
                    }
                    else if (typeToProcess.IsEnum)
                    {
                        enumProcessor.ProcessType(typeToProcess);
                    }
                    else
                    {
                        if (!context.ModelsToIgnore.Contains(typeToProcess))
                        {
                            modelProcessor.ProcessType(typeToProcess);
                        }
                    }
                });
                nextBatch = context.GetNextBatch();
            }

            // Update index files
            Console.WriteLine("Writing Controllers...");
            controllerProcessor.UpdateIndex();

            Console.WriteLine("Writing Enums...");
            enumProcessor.UpdateIndex();

            Console.WriteLine("Writing Models...");
            modelProcessor.UpdateIndex();
        }
    }
}
