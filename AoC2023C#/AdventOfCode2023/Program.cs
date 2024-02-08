namespace AdventOfCode2023
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO: Implement interface on day 7 and earlier

            bool test = false;
            int testCount = 0;
            int dayNumber = 25;
            int part = 0;
            try
            {
                IDay day = new Day25();
                string filepath = CreateFilepath(dayNumber, test, testCount);

                day.Execute(filepath, part);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.ToString());
            }
        }

        static string CreateFilepath(int day, bool testNotProd, int testCount)
        {
            string filename;
            if (testNotProd)
            {
                if (testCount == 0)
                {
                    filename = "test.txt";
                }
                else
                {
                    filename = $"test{testCount}.txt";
                }
            }
            else
            {
                filename = "data.txt";
            }
            return $"C:\\Misc\\AdventOfCode2023\\Day{day}\\{filename}";
        }
    }
}
