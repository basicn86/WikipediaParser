using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikipediaParser
{
    static class WikipediaReadBuffer
    {
        //queue of WikipediaPage
        private static Queue<WikipediaPage> buffer = new Queue<WikipediaPage>();
        //lock object
        private static readonly object lockObject = new object();
        //max buffer size 1GB
        private const uint MAX_BUFFER_SIZE = 1 * 1024 * 1024 * 1024;
        private static uint currentBufferSize = 0;
        private static bool EOF = false;

        //add page to buffer
        public static void AwaitEnqueue(WikipediaPage page, CancellationTokenSource cancellationTokenSource)
        {
            //check if the current buffer size is bigger than the max buffer size, if it is, wait until the buffer is smaller
            while (currentBufferSize > MAX_BUFFER_SIZE)
            {
                Thread.Sleep(100);
            }

            //add page to buffer
            lock (lockObject)
            {
                currentBufferSize += (uint)page.text.Length;
                currentBufferSize += (uint)page.title.Length;
                buffer.Enqueue(page);
            }
        }

        //dequeue page from buffer
        public static WikipediaPage Dequeue()
        {
            lock (lockObject)
            {
                //check if buffer is empty
                if (buffer.Count == 0) throw new Exception("Buffer is empty");

                WikipediaPage page = buffer.Dequeue();

                //remove the size of the page from the current buffer size
                currentBufferSize -= (uint)page.text.Length;
                currentBufferSize -= (uint)page.title.Length;

                return page;
            }
        }

        public static WikipediaPage AwaitForPage()
        {
            while (true)
            {
                lock (lockObject)
                {
                    if (buffer.Count > 0)
                    {
                        WikipediaPage page = buffer.Dequeue();
                        currentBufferSize -= (uint)page.text.Length;
                        currentBufferSize -= (uint)page.title.Length;
                        return page;
                    } else if (EOF)
                    {
                        throw new EndOfStreamException("End of File");
                    }
                }

                //await if the buffer is empty
                Thread.Sleep(100);
            }
        }

        //notify EOF function
        public static void NotifyEOF()
        {
            EOF = true;
        }
    }
}
