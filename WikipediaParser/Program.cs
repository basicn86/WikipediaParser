namespace WikipediaParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //create new WikipediaLoader
            WikipediaLoader loader = new WikipediaLoader("E:/enwiki.xml");
            //writer
            WikipediaXmlWriter writer = new WikipediaXmlWriter("D:/enwiki/parsedwiki.xml");

            //start thread
            loader.StartThread();
            writer.StartThread();

            //wait 10 seconds
            await Task.Delay(10000);

            //try dequeueing all pages from the buffer
            while (true)
            {
                try
                {
                    //cancellation source
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    WikipediaPage page = await WikipediaReadBuffer.AwaitForPage(cancellationTokenSource);

                    page.text = "";

                    await WikipediaWriteBuffer.Enqueue(page);

                } catch (Exception e)
                {
                    //if the loader reached EOF, await 10 seconds and return
                    if (loader.IsEOF())
                    {
                        await Task.Delay(10000);
                        writer.CancelThread();
                        return;
                    }
                }
            }
        }
    }
}