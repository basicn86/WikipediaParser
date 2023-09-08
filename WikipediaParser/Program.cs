namespace WikipediaParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //create new WikipediaLoader
            WikipediaLoader loader = new WikipediaLoader("C:/enwiki.xml");
            //writer
            WikipediaXmlWriter writer = new WikipediaXmlWriter("D:/enwiki/parsedwiki.xml");

            //start thread
            loader.StartThread();
            writer.StartThread();

            //wait 10 seconds
            await Task.Delay(3000);

            //try dequeueing all pages from the buffer
            while (true)
            {
                try
                {
                    //cancellation source
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    WikipediaPage page = WikipediaReadBuffer.AwaitForPage();

                    page.text = "";

                    WikipediaWriteBuffer.AwaitEnqueue(page);
                } catch (EndOfStreamException e)
                {
                    //write to console that the end of the stream has been reached
                    Console.WriteLine("End of stream reached, reader is finishing");

                    //wait for writer and loader to finish threads
                    loader.CancelThread();
                    writer.CancelThread();
                    return;
                } catch (Exception e)
                {
                    //write to console a critical error occured
                    Console.WriteLine("Critical error: " + e.Message);
                    return;
                }
            }
        }
    }
}