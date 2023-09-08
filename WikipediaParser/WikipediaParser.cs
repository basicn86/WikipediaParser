using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WikipediaParser
{
    public class WikipediaParser
    {
        private Thread? thread = null;

        public WikipediaParser()
        {

        }

        private static string FilterSpecialCharacters(string input)
        {
            //regex to remove every character except for letters and spaces
            Regex regex = new Regex("[^a-zA-Z ]");
            Regex whitespaceRegex = new Regex(@"\s+");

            string result = regex.Replace(input, " ");
            result = whitespaceRegex.Replace(result, " ");

            return result;
        }

        private void Parse()
        {
            while (true)
            {
                try
                {
                    //await page from buffer
                    WikipediaPage page = WikipediaReadBuffer.AwaitForPage();

                    if (page != null)
                    {
                        page.text = FilterSpecialCharacters(page.text);
                    }

                    //write the page to the xml file
                    WikipediaWriteBuffer.AwaitEnqueue(page);
                } catch (Exception e) {
                    Console.WriteLine("Thread encountered an exception");
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public void Start()
        {
            if (thread is not null) return;
            thread = new Thread(Parse);
            thread.Start();
        }

        public void Join()
        {
            if (thread is null) return;

            thread.Join();
        }
    }
}
