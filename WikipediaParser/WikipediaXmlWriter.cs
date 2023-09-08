using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WikipediaParser
{
    public class WikipediaXmlWriter
    {
        private string xmlPath = "";
        private Thread? thread = null;

        public WikipediaXmlWriter(string xmlPath)
        {
            this.xmlPath = xmlPath;
        }

        private void WriteXml()
        {
            //xml writer settings
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Async = false,
                Encoding = Encoding.UTF8,
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(null, "wiki", null);

                try
                {
                    while (!WikipediaWriteBuffer.IsClosed() || !WikipediaWriteBuffer.IsEmpty())
                    {
                        //await page from buffer
                        WikipediaPage page = WikipediaWriteBuffer.AwaitForPage();

                        //write the page to the xml file
                        writer.WriteStartElement(null, "page", null);
                        writer.WriteElementString(null, "title", null, page.title);
                        writer.WriteElementString(null, "text", null, page.text);
                        writer.WriteEndElement();
                    }
                }
                catch (Exception e)
                {
                    //write to console
                    Console.WriteLine("Critical error: " + e.Message);
                }
                

                //write the end of the document
                writer.WriteEndElement();
                writer.WriteEndDocument();

                //close the writer and flush the remaining data
                writer.Flush();
                writer.Close();
            }

            //write to the console that the thread has been completed
            Console.WriteLine("WriteXml has been completed");
        }

        public void StartThread()
        {
            //return if thread is already going
            if (thread is not null) return;

            //start thread
            thread = new Thread(() =>
            {
                WriteXml();
            });
            thread.Start();

            Console.WriteLine("Writer has been started");
        }

        //cancel thread function
        public void CancelThread()
        {
            WikipediaWriteBuffer.Close();

            //wait for thread to finish
            thread.Join();
        }
    }
}
