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
        private Thread thread;
        private CancellationTokenSource? cancellationTokenSource;

        public WikipediaXmlWriter(string xmlPath)
        {
            this.xmlPath = xmlPath;
        }

        private async Task WriteXml()
        {
            //xml writer settings
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
            {
                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(null, "wiki", null);

                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        //await page from buffer
                        WikipediaPage page = await WikipediaWriteBuffer.AwaitForPage(cancellationTokenSource);

                        //write the page to the xml file
                        await writer.WriteStartElementAsync(null, "page", null);
                        await writer.WriteElementStringAsync(null, "title", null, page.title);
                        await writer.WriteElementStringAsync(null, "text", null, page.text);
                        await writer.WriteEndElementAsync();
                    }
                }
                catch (EndOfStreamException e)
                {
                    //write to the console that the end of the stream has been reached
                    Console.WriteLine("End of stream reached, writer is finishing");

                    //finish the writer
                    await writer.WriteEndElementAsync();
                    await writer.WriteEndDocumentAsync();
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Critical error in writer: " + e.Message);
                }

                //finish the writer if the cancellation token has been requested
                await writer.WriteEndElementAsync();
                await writer.WriteEndDocumentAsync();
            }

            //write to the console that the thread has been completed
            Console.WriteLine("WriteXml has been completed");
        }

        public void StartThread()
        {
            //return if thread is already going
            if (cancellationTokenSource is not null) return;
            cancellationTokenSource = new CancellationTokenSource();

            //start thread
            thread = new Thread(() =>
            {
                WriteXml();
            });
            thread.Start();
        }

        //cancel thread function
        public void CancelThread()
        {
            //cancel the token
            cancellationTokenSource?.Cancel();

            //wait for thread to finish
            thread.Join();
        }
    }
}
