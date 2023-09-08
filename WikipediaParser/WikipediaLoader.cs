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
        private volatile bool isRunning = false;

        public WikipediaLoader(string xmlPath)
        {
            this.xmlPath = xmlPath;
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
                    if (!isRunning) return;

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
                        await WikipediaReadBuffer.Enqueue(page);
                    }
                }
            }
        }

        public void StartThread()
        {
            //return if thread is already going
            if (isRunning) return;

            //start thread
            thread = new Thread(() =>
            {
                LoadXml();
            });
            isRunning = true;
            thread.Start();
        }

        public void CancelThread()
        {
            isRunning = false;
        }
    }
}
