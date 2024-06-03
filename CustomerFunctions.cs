using DapperApp.Repository;

namespace DapperApp
{
    public static class CustomerFunctions
    {
        public static async Task ListAllCompanies(string connectionString)
        {
            var repository = new CompanyRepository(connectionString);
            var companies = await repository.GetCompanyInfosAsync();

            foreach (var company in companies)
            {
                Console.WriteLine($"Id: {company.Id}, Name: {company.Name}, Created: {company.Created}, Modified: {company.Modified}");
                Console.WriteLine($"Address: {company.Street}, {company.City}, {company.State}, {company.PostalCode}");
                Console.WriteLine();
            }
        }

        public static async Task ListCompaniesWithPaging(string connectionString, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new Exception("Invalid page size");
            }

            var repository = new CompanyRepository(connectionString);
            var companies = await repository.GetCompanyInfosAsync();

            var pages = (int)Math.Ceiling(((decimal)companies.Count() / (decimal)pageSize));

            for (int i = 0; i < pages; i++)
            {
                Console.WriteLine("Paging results");
                Console.WriteLine($"Page {i + 1}");

                var companyPage = companies.Skip(i * pageSize).Take(pageSize);

                foreach (var company in companyPage)
                {
                    OutputOneCompany(company);
                }

                Console.WriteLine("Press ENTER for the next page");
                Console.ReadLine();
            }

            Console.WriteLine("Paging complete");

            static void OutputOneCompany(Repository.Company company)
            {
                Console.WriteLine($"Id: {company.Id}, Name: {company.Name}, Created: {company.Created}, Modified: {company.Modified}");
                Console.WriteLine($"Address: {company.Street}, {company.City}, {company.State}, {company.PostalCode}");
                Console.WriteLine();
            }
        }

        public static async Task ListCompaniesWithSQLPaging(string connectionString)
        {
            int pageSize = 10;
            int currentPage = 1;
            bool continuePaging = true;

            var repository = new CompanyRepository(connectionString);

            while (continuePaging)
            {
                var companies = await repository.GetPagedCompaniesAsync(currentPage, pageSize);

                Console.WriteLine($"Page {currentPage}:\n");
                continuePaging = companies.Count() == pageSize;
                foreach (var company in companies)
                {
                    Console.WriteLine($"Id: {company.Id}, Name: {company.Name}, Created: {company.Created}, Modified: {company.Modified}");
                    Console.WriteLine($"Address: {company.Street}, {company.City}, {company.State}, {company.PostalCode}");
                    Console.WriteLine();
                }

                if (companies.Any())
                {
                    Console.WriteLine("Enter 'n' for next page, 'p' for previous page, or 'q' to quit:");
                    var input = Console.ReadLine();

                    if (input == "n")
                    {
                        currentPage++;
                    }
                    else if (input == "p" && currentPage > 1)
                    {
                        currentPage--;
                    }
                    else if (input == "q")
                    {
                        continuePaging = false;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input, please try again");
                    }
                }

            }
        }

        public static async Task DeleteAllCompanies(string connectionString)
        {
            var repository = new CompanyRepository(connectionString);
            await repository.DeleteAllCompaniesAsync();
        }

        public static async Task ViewCustomerById(string connectionString)
        {
            Console.WriteLine("Enter the ID of the customer:");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var repository = new CompanyRepository(connectionString);
                var customer = await repository.GetCompanyByIdAsync(id);

                if (customer != null)
                {
                    Console.WriteLine($"Customer found:");
                    Console.WriteLine($"Id: {customer.Id}, Name: {customer.Name}, Created: {customer.Created}, Modified: {customer.Modified}");
                    Console.WriteLine($"Address: {customer.Street}, {customer.City}, {customer.State}, {customer.PostalCode}\n");
                    Console.WriteLine($"isEnabled = {customer.IsActive}");
                    // Add ability to toggle the isEnabled field/column or whatever
                    Console.WriteLine("Press T to toggle the isEnabled flag, or any other key to exit");
                    var answer = Console.ReadKey();
                    if (answer.KeyChar == 't' || answer.KeyChar == 'T')
                    {
                        await repository.ToggleIsEnabledAsync(id);
                        Console.WriteLine("\nIs active is toggled");
                    }



                }
                else
                {
                    Console.WriteLine($"No customer found with ID: {id}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid ID.");
            }
        }


        public static async Task ToggleIsActive(string connectionString)
        {

        }
    }
}


