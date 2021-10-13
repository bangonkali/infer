using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Infer
{
  class Program
  {
    static int Main(string[] args)
    {
      var xmlDirectory = args[0];
      // var xsdOutput = args[1];

      XmlSchemaSet schemaSet = new XmlSchemaSet();
      XmlSchemaInference inference = new XmlSchemaInference();

      var consumedFiles = 0;

      if (!Directory.Exists(xmlDirectory))
      {
        Console.WriteLine("Directory must exist and must contain xml files.");
        return -1;
      }

      var files = Directory.EnumerateFiles(xmlDirectory)
        .Where(f => f.EndsWith(".xml"))
        .Where(IsValidXml);

      foreach (var file in files)
      {
        try
        {
          XmlReader reader = XmlReader.Create(file);
          schemaSet = consumedFiles == 0 ? inference.InferSchema(reader) : inference.InferSchema(reader, schemaSet);
          consumedFiles++;
        }
        catch (UnauthorizedAccessException ex)
        {
          Console.WriteLine(ex.Message);
        }
        catch (PathTooLongException ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      if (consumedFiles == 0)
      {
        Console.WriteLine("No xml files consumed for inference");
        return -1;
      }

      var count = schemaSet.Schemas().Count;

      if (count == 1)
      {
        foreach (XmlSchema schema in schemaSet.Schemas())
        {
          schema.Write(Console.Out);
        }
      }
      else
      {
        Console.WriteLine($"Schemas found: {count}");
        return -1;
      }

      return 0;
    }

    static bool IsValidXml(string file)
    {
      try
      {
        XDocument xd1 = XDocument.Load(file);
        Debug.WriteLine($"File {xd1} is valid");
        return true;
      }
      catch (XmlException exception)
      {
        Debug.WriteLine($"File {file} is invalid. {exception.Message}.");
        return false;
      }
    }
  }
}