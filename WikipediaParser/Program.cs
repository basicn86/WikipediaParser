namespace WikipediaParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //create new WikipediaLoader
            WikipediaLoader loader = new WikipediaLoader("E:/enwiki.xml");

            //start thread
            loader.StartThread();

            //wait 10 seconds
            await Task.Delay(10000);

            //try dequeueing all pages from the buffer
            while (true)
            {
                WikipediaPage page = await WikipediaReadBuffer.AwaitForPage();
                Console.WriteLine(page.title);
            }
        }
    }
}