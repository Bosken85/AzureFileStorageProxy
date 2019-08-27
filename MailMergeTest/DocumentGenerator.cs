using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;

namespace MailMergeTest
{
    public class DocumentGenerator
    {
        public static WordDocument Generate(TemplateData templateData)
        {
            using (var fileStreamPath = new FileStream($"{templateData.Template}.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var document = new WordDocument(fileStreamPath, FormatType.Docx);

                IEnumerable<object> data = templateData.Data.GetType().GetInterface(typeof(IEnumerable<>).FullName) != null
                    ? templateData.Data as IEnumerable<object>
                    : new[] { templateData.Data };

                MailMergeDataTable dataTable = new MailMergeDataTable(templateData.Template, data);
                document.MailMerge.ExecuteNestedGroup(dataTable);
                foreach (var subTemplate in templateData.SubTemplates)
                {
                    var subDocument = Generate(subTemplate);
                    document.Replace($"<template:{subTemplate.Template}>", subDocument, false, true, false);
                }
                document.Replace(new Regex(@"<template:.+>"), string.Empty);
                return document;
            }
        }
    }

    public class TemplateData
    {
        public string Template { get; set; }
        public object Data { get; set; }
        public IEnumerable<TemplateData> SubTemplates { get; set; } = new List<TemplateData>();

        public TemplateData(string template)
        {
            Template = template;
        }

        public TemplateData(string template, object data)
        {
            Template = template;
            Data = data;
        }
    }
}
