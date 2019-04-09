using AuthorRightClaim.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorRight
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Author Right Claim data Import....");
            string filePath = Utilities.ReadConfigFile("Path");
            Console.WriteLine("Checking file Path : " + filePath);
            string[] files = Directory.GetFiles(filePath, "*.XML", SearchOption.TopDirectoryOnly);
            try
            {
                if (filePath != "" && Directory.Exists(filePath))
                {
                    foreach (string file in files)
                    {
                        Console.WriteLine("Reading file : " + file);
                        bool success = ReadXml.getXmlData(file);

                        if (success)
                        {
                            string fileName = Path.GetFileName(file);
                            string desPath = filePath + @"Processed\" + DateTime.Now.ToString("yyyyMMddHHmmss") + @"\";

                            if (!Directory.Exists(desPath))
                            {
                                Directory.CreateDirectory(desPath);
                            }
                            File.Move(file, desPath + fileName);
                            Console.WriteLine("Data Import complete");
                        }
                        else
                        {
                            Utilities.sendEmail("Data import failed!");
                        }
                    }
                }
                else
                {
                    Utilities.sendEmail("Invalid file path!");
                }
            }
            catch (Exception ex)
            {
                Utilities.sendEmail("Exception Occurred : " + ex);
            }
        }
    }
}
