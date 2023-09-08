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

            WikipediaParser[] parsers = new WikipediaParser[12];
            for (int i = 0; i < 12; i++)
            {
                parsers[i] = new WikipediaParser();
                parsers[i].Start();
            }

            //wait for all parsers to finish
            for (int i = 0; i < 12; i++)
            {
                parsers[i].Join();
            }

            loader.CancelThread();
            writer.CancelThread();
        }
    }
}