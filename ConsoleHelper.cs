namespace DapperApp;

public static class ConsoleHelper
{
    public static string ReadValidResponse(string prompt, params string[] validResponses)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            throw new Exception("Prompt is required");
        }

        if (validResponses.Length == 0)
        {
            throw new Exception("A valid response is required");
        }

        if (validResponses.Any(vr => string.IsNullOrEmpty(vr)))
        {
            throw new Exception("Valid responses may not be empty");
        }

        bool gotValidResponse = false;
        var promptParts = prompt.Split('\r', StringSplitOptions.TrimEntries);

        foreach (var p in promptParts)
        {
            Console.WriteLine(p);
        }

        while (!gotValidResponse)
        {
            var response = Console.ReadLine();
            gotValidResponse = validResponses.Any(vr => vr == response);

            if (!gotValidResponse)
            {
                var previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("That's not a valid choice");
                Console.ForegroundColor = previousColor;
            }
            else
            {
                return response ?? throw new Exception("This is not Possible");
            }
        }

        throw new Exception("This is not possible");


    }
}
