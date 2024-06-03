using Dapper;
using DapperApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DapperApp.Repository;

public sealed record class Company
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime Modified { get; set; }
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required bool IsActive { get; set; }
    public required string CreatedBy { get; set; }
    public required string ModifiedBy { get; set; }
}



public class CompanyRepository
{
    private readonly string _connectionString;

    public CompanyRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public CompanyRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Company") ?? throw new Exception("The Connection string is not configured");

        _connectionString = "Data Source=.;Database=CompanyDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";

    }

    public async Task<IEnumerable<CompanyInfos>> GetCompanyInfosAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM CompanyInfos";
        return await connection.QueryAsync<CompanyInfos>(sql);
    }

    public async Task InsertCompanies(IEnumerable<Company> companies)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
                INSERT INTO Companies (Name, Created, Modified, Street, City, State, PostalCode)
                VALUES (@Name, @Created, @Modified, @Street, @City, @State, @PostalCode)";


        foreach (var company in companies)
        {
            await connection.ExecuteAsync(sql, company);
        }
    }

   

    public async Task<IEnumerable<Company>> GetPagedCompaniesAsync(int pageNumber, int pageSize)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
                SELECT * FROM Companies
                ORDER BY Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
        return await connection.QueryAsync<Company>(sql, parameters);
    }

    public async Task DeleteAllCompaniesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "DELETE FROM Companies";
        await connection.ExecuteAsync(sql);
    }

    public async Task<Company?> GetCompanyByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM Companies WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Company>(sql, new { Id = id });
    }

    public async Task ToggleIsEnabledAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
                UPDATE Companies 
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE Id = @id";
        await connection.ExecuteAsync(sql, new { id });

    }

    public async Task DeleteCompanyByIdAsync(int companyId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
        UPDATE Companies 
        SET IsDeleted = 1, Modified = @Modified, ModifiedBy = @ModifiedBy 
        WHERE Id = @Id";

        var parameters = new { Modified = DateTime.UtcNow, ModifiedBy = "Kyle", Id = companyId };
        await connection.ExecuteAsync(sql, parameters);
    }


    public async Task<Company> InsertCompanyAsync(Company company)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
        INSERT INTO Companies (Name, Created, Modified, Street, City, State, PostalCode, IsActive, CreatedBy, ModifiedBy)
        VALUES (@Name, @Created, @Modified, @Street, @City, @State, @PostalCode, @IsActive, @CreatedBy, @ModifiedBy);
        SELECT CAST(SCOPE_IDENTITY() as int)";

        company.Created = DateTime.UtcNow;
        company.Modified = DateTime.UtcNow;
        company.CreatedBy = "Conor"; // Hard-coded as per requirements
        company.ModifiedBy = "Conor"; // Hard-coded as per requirements

        var id = await connection.QuerySingleAsync<int>(sql, company);
        company.Id = id;
        return company;
    }


    public async Task<Company?> UpdateCompanyAsync(Company company)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Fetch the existing company to preserve Created and CreatedBy fields
        var existingCompany = await GetCompanyByIdAsync(company.Id);
        if (existingCompany == null)
        {
            return null;
        }

        company.Created = existingCompany.Created;
        company.CreatedBy = existingCompany.CreatedBy;
        company.Modified = DateTime.UtcNow;
        company.ModifiedBy = "Kyle"; // Hard-coded as per requirements

        string sql = @"
        UPDATE Companies
        SET Name = @Name,
            Modified = @Modified,
            Street = @Street,
            City = @City,
            State = @State,
            PostalCode = @PostalCode,
            IsActive = @IsActive,
            ModifiedBy = @ModifiedBy
        WHERE Id = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, company);
        return affectedRows > 0 ? company : null;
    }
    public async Task<IEnumerable<CompanyCountByState>> GetCompanyCountByStateAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
                SELECT State, COUNT(*) AS CompanyCount
                FROM Companies
                WHERE IsActive = 1
                GROUP BY State
                ORDER BY CompanyCount DESC";

        var result = await connection.QueryAsync<CompanyCountByState>(sql);
        return result;
    }

    public async Task<IEnumerable<Company>> GetDeletedCompaniesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "SELECT * FROM Companies WHERE IsDeleted = 1";
        return await connection.QueryAsync<Company>(sql);
    }

    public async Task<bool> UndeleteCompanyAsync(int companyId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "UPDATE Companies SET IsDeleted = 0 WHERE Id = @CompanyId";
        var affectedRows = await connection.ExecuteAsync(sql, new { CompanyId = companyId });
        return affectedRows > 0;
    }

    public async Task ExecuteDeleteOldDeletedCompaniesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "EXEC sp_DeleteOldDeletedCompanies";
        await connection.ExecuteAsync(sql);
    }
}



