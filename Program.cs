using Bogus;
using DapperApp;
using DapperApp.Repository;
using System.Globalization;



class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "Data Source=.;Database=CompanyDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";

        // detect invariant culture
        var invariantCulture = CultureInfo.GetCultureInfo("en-US").EnglishName.Contains("Invariant");
        if (invariantCulture)
        {
            Console.WriteLine("Invariant culture is on");
        }

        while (true)
        {
            try
            {
                var prompt = "Company Menu:\n1. List all Customers\n2. Insert 100 Customers\n3. Page through customers (LINQ)\n4. Page through customers (SQL) \n5. Delete All Companies\n6. Get a Customer by Id \n7. Exit\n";
                var validResponse = ReadValidResponse(prompt, "1", "2", "3", "4", "5", "6", "7");

                switch (validResponse)
                {
                    case "1":
                        await CustomerFunctions.ListAllCompanies(connectionString);
                        break;
                    case "2":
                        await InsertFakeCompanies(connectionString);
                        Console.WriteLine("Inserted 100 fake companies data into the database.");
                        break;
                    case "3":
                        await CustomerFunctions.ListCompaniesWithPaging(connectionString, 11);
                        break;
                    case "4":
                        await CustomerFunctions.ListCompaniesWithSQLPaging(connectionString);
                        break;
                    case "5":
                        await CustomerFunctions.DeleteAllCompanies(connectionString);
                        break;
                    case "6":
                        await CustomerFunctions.ViewCustomerById(connectionString);
                        break;
                    case "7":
                        return; // Exit the program
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }


    static async Task InsertFakeCompanies(string connectionString)
    {
        var faker = new Faker<Company>()
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Created, f => f.Date.Past())
            .RuleFor(c => c.Modified, f => f.Date.Recent())
            .RuleFor(c => c.Street, f => f.Address.StreetAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.State, f => f.Address.State())
            .RuleFor(c => c.PostalCode, f => f.Address.ZipCode());

        List<Company> companies = faker.Generate(100);

        var repository = new CompanyRepository(connectionString);
        await repository.InsertCompanies(companies);
    }

    static string ReadValidResponse(string prompt, params string[] validResponses)
    {
        string response;
        do
        {
            Console.WriteLine(prompt);
            response = Console.ReadLine()?.Trim() ?? "";
        } while (!Array.Exists(validResponses, v => v.Equals(response, StringComparison.OrdinalIgnoreCase)));
        return response;
    }
}

