using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DapperApp.Repository
{
    public sealed record class CompanyContact
    {
        public required int Id { get; set; }
        public required int CompanyId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid Email Format")]
        public required string Email { get; set; }

        [StringLength(15, ErrorMessage = "Mobile phone number cannot be longer than 15 characters.")]
        public required string MobilePhone { get; set; }

        [StringLength(15, ErrorMessage = "Office phone number cannot be longer than 15 characters.")]
        public required string OfficePhone { get; set; }

        public required bool IsDeleted { get; set; }

        public required DateTime Created { get; set; }
        public required DateTime Modified { get; set; }
        public required string CreatedBy { get; set; }
        public required string ModifiedBy { get; set; }
    }

    public class CompanyContactsRepository
    {
        private readonly string _connectionString;

        public CompanyContactsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Company") ?? throw new Exception("The Connection string is not configured");
        }

        public async Task<IEnumerable<CompanyContact>> GetContactsByCompanyIdAsync(int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM CompanyContacts WHERE CompanyId = @CompanyId AND IsDeleted = 0";
            return await connection.QueryAsync<CompanyContact>(sql, new { CompanyId = companyId });
        }

        public async Task<CompanyContact?> GetContactByIdAsync(int contactId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM CompanyContacts WHERE Id = @Id AND IsDeleted = 0";
            return await connection.QuerySingleOrDefaultAsync<CompanyContact>(sql, new { Id = contactId });
        }

        public async Task<CompanyContact> InsertContactAsync(CompanyContact contact)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
        INSERT INTO CompanyContacts (CompanyId, Name, Email, MobilePhone, OfficePhone, IsDeleted, Created, Modified, CreatedBy, ModifiedBy)
        VALUES (@CompanyId, @Name, @Email, @MobilePhone, @OfficePhone, 0, @Created, @Modified, @CreatedBy, @ModifiedBy);
        SELECT CAST(SCOPE_IDENTITY() as int)";

            contact.Created = DateTime.UtcNow;
            contact.Modified = DateTime.UtcNow;
            contact.CreatedBy = "Conor"; // Hard-coded as per requirements
            contact.ModifiedBy = "Conor"; // Hard-coded as per requirements

            var id = await connection.QuerySingleAsync<int>(sql, contact);
            return contact with { Id = id };
        }

        public async Task<CompanyContact?> UpdateContactAsync(CompanyContact contact)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch existing contact to preserve Created and CreatedBy fields
            var existingContact = await GetContactByIdAsync(contact.Id);
            if (existingContact == null)
            {
                return null;
            }

            contact.Created = existingContact.Created;
            contact.CreatedBy = existingContact.CreatedBy;
            contact.Modified = DateTime.UtcNow;
            contact.ModifiedBy = "Kyle"; // Hard-coded as per requirements

            string sql = @"
        UPDATE CompanyContacts
        SET Name = @Name,
            Email = @Email,
            MobilePhone = @MobilePhone,
            OfficePhone = @OfficePhone,
            Modified = @Modified,
            ModifiedBy = @ModifiedBy
        WHERE Id = @Id AND IsDeleted = 0";

            var affectedRows = await connection.ExecuteAsync(sql, contact);
            return affectedRows > 0 ? contact : null;
        }

        public async Task<bool> SoftDeleteContactAsync(int contactId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
        UPDATE CompanyContacts
        SET IsDeleted = 1,
            Modified = @Modified,
            ModifiedBy = @ModifiedBy
        WHERE Id = @Id";

            var parameters = new { Modified = DateTime.UtcNow, ModifiedBy = "Kyle", Id = contactId };
            var affectedRows = await connection.ExecuteAsync(sql, parameters);
            return affectedRows > 0;
        }
    }
}
