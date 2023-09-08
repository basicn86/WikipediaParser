using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WikipediaParser
{
    public class WikipediaLoader
    {
        private string xmlPath = "";
        private Thread thread;
        //cancellation token
        private CancellationTokenSource? cancellationTokenSource;
        private bool EOF = false;

        public WikipediaLoader(string xmlPath)
        {
            this.xmlPath = xmlPath;
            this.EOF = false;
        }

        private async Task LoadXml()
        {
            //create new XmlReader
            using (XmlReader reader = XmlReader.Create(xmlPath))
            {
                //read until the end of the file
                while (reader.Read())
                {
                    //if isRunning is false, stop the thread
                    if (cancellationTokenSource.IsCancellationRequested) return;

                    //check if the current node is an element
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    //check if the current element is a page
                    if (reader.Name == "page")
                    {
                        //create new WikipediaPage
                        WikipediaPage page = new WikipediaPage();

                        while(reader.NodeType != XmlNodeType.Element || reader.Name != "title") { reader.Read(); }

                        //read the title
                        page.title = reader.ReadElementContentAsString();

                        while (reader.NodeType != XmlNodeType.Element || reader.Name != "text") { reader.Read(); }

                        //read the text
                        page.text = reader.ReadElementContentAsString();

                        //if the page is a redirect page, skip it
                        if (page.text.StartsWith("#REDIRECT")) continue;

                        //add the page to the buffer
                        WikipediaReadBuffer.AwaitEnqueue(page, cancellationTokenSource);
                    }
                }
            }

            //notify the buffer that the end of the file has been reached
            WikipediaReadBuffer.NotifyEOF();

            //remove the cancellation source
            cancellationTokenSource = null;

            //write to console end of buffer
            Console.WriteLine("End of buffer");
        }

        public void StartThread()
        {
            //return if thread is already going
            if (cancellationTokenSource is not null) return;
            cancellationTokenSource = new CancellationTokenSource();

            //start thread
            thread = new Thread(() =>
            {
                LoadXml();
            });
            thread.Start();
        }

        public void CancelThread()
        {
            cancellationTokenSource?.Cancel();

            //wait for thread to join
            thread.Join();
        }
    }
}
