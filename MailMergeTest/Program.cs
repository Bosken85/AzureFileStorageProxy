using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailMergeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = GetTemplate();
            using (MemoryStream ms = new MemoryStream())
            using (var document = DocumentGenerator.Generate(data))
            {
                document.Save(ms, FormatType.Docx);
                var bytes = ms.ToArray();
                var storageClient = new StorageClient();
                var task = storageClient.UploadFileToBlobAsync("kevin", "result.docx", bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                var url = task.Result;
            }
        }

        static TemplateData GetTemplate()
        {
            return new TemplateData("Employees")
            {
                Data = new
                {
                    Name = "Bosteels",
                    FirstName = "Kevin",
                    Gender = "male",
                    Orders = new[]
                    {
                        new
                        {
                            Name = "Order 1", Total = 10.5M, OrderLines = new[]
                            {
                                new {Name = "Product 1", Price = 3.5M, Amount = 1},
                                new {Name = "Product 2", Price = 4.5M, Amount = 2},
                                new {Name = "Product 3", Price = 5.5M, Amount = 3}

                            }
                        },
                        new
                        {
                            Name = "Order 2", Total = 11.5M, OrderLines = new[]
                            {
                                new {Name = "Product 4", Price = 6.5M, Amount = 4},
                                new {Name = "Product 5", Price = 7.5M, Amount = 5},
                                new {Name = "Product 6", Price = 8.5M, Amount = 6}
                            }
                        },
                        new
                        {
                            Name = "Order 3", Total = 12.5M, OrderLines = new[]
                            {
                                new {Name = "Product 7", Price = 9.5M, Amount = 7},
                                new {Name = "Product 8", Price = 10.5M, Amount = 8},
                                new {Name = "Product 9", Price = 11.5M, Amount = 9}
                            }
                        }
                    }
                },
                SubTemplates = new[]
                {
                    new TemplateData("Products")
                    {
                        Data = new[]
                        {
                            new
                            {
                                Name = "Order 1", Total = 10.5M, OrderLines = new[]
                                {
                                    new {Name = "Product 1", Price = 3.5M, Amount = 1},
                                    new {Name = "Product 2", Price = 4.5M, Amount = 2},
                                    new {Name = "Product 3", Price = 5.5M, Amount = 3}

                                }
                            },
                            new
                            {
                                Name = "Order 2", Total = 11.5M, OrderLines = new[]
                                {
                                    new {Name = "Product 4", Price = 6.5M, Amount = 4},
                                    new {Name = "Product 5", Price = 7.5M, Amount = 5},
                                    new {Name = "Product 6", Price = 8.5M, Amount = 6}
                                }
                            },
                            new
                            {
                                Name = "Order 3", Total = 12.5M, OrderLines = new[]
                                {
                                    new {Name = "Product 7", Price = 9.5M, Amount = 7},
                                    new {Name = "Product 8", Price = 10.5M, Amount = 8},
                                    new {Name = "Product 9", Price = 11.5M, Amount = 9}
                                }
                            }
                        }
                    },
                }
            };
        }
    }
}
