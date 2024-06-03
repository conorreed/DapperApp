using Bogus;
using DapperApp.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/Companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyRepository repo;
        private readonly CompanyContactsRepository contactsRepo;

        public CompaniesController(CompanyRepository repo, CompanyContactsRepository contactsRepo)
        {
            this.repo = repo;
            this.contactsRepo = contactsRepo;
        }

        // GET: /Company
        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await repo.GetCompanyInfosAsync();
            return Ok(companies);
        }

        // GET: /Company/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await repo.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        // POST: /Company/{id}/disable
        [HttpPost("{id}/disable")]
        public async Task<IActionResult> DisableCompany(int id)
        {
            var company = await repo.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            if (!company.IsActive)
            {
                return BadRequest("This company is already disabled");
            }

            await repo.ToggleIsEnabledAsync(id);

            company = await repo.GetCompanyByIdAsync(id);

            return Ok(company);
        }

        [HttpPost("{id}/enable")]
        public async Task<IActionResult> EnableCompany(int id)
        {
            var company = await repo.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            if (company.IsActive)
            {
                return BadRequest("This company is already enabled");
            }

            await repo.ToggleIsEnabledAsync(id);

            company = await repo.GetCompanyByIdAsync(id);

            return Ok(company);
        }

        // make routes for deleting all companies, inserting 100 companies, and deleting a specific company by ID
        // DELETE: /Companies
        [HttpDelete]
        public async Task<IActionResult> DeleteAllCompanies()
        {
            await repo.DeleteAllCompaniesAsync();
            return NoContent();
        }

        // POST: /Companies/insert-fake
        [HttpPost("insert-fake")]
        public async Task<IActionResult> InsertFakeCompanies()
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

            await repo.InsertCompanies(companies);

            return Ok($"Inserted {companies.Count} fake companies into the database.");
        }

        // DELETE: /Companies/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyById(int id)
        {
            var company = await repo.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            // Assuming you have a method to delete a company by Id in your repository
            await repo.DeleteCompanyByIdAsync(id);

            return NoContent();
        }

        // POST: /api/Companies
        [HttpPost]
        public async Task<IActionResult> InsertCompany([FromBody] Company company)
        {
            if (company.Id != 0)
            {
                return BadRequest("The Id property must be 0 for a new company.");
            }

            company.Created = DateTime.UtcNow;
            company.Modified = DateTime.UtcNow;
            company.CreatedBy = "Conor";
            company.ModifiedBy = "Conor";
            company.IsActive = true;

            var createdCompany = await repo.InsertCompanyAsync(company);

            return CreatedAtAction(nameof(GetCompanyById), new { id = createdCompany.Id }, createdCompany);
        }


        // PUT: /api/Companies/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company company)
        {
            if (company.Id != id)
            {
                return BadRequest("The Id in the path does not match the Id in the company object.");
            }

            var existingCompany = await repo.GetCompanyByIdAsync(id);
            if (existingCompany == null)
            {
                return NotFound();
            }

            company.Created = existingCompany.Created;
            company.CreatedBy = existingCompany.CreatedBy;
            company.Modified = DateTime.UtcNow;
            company.ModifiedBy = "Kyle";

            var updatedCompany = await repo.UpdateCompanyAsync(company);
            if (updatedCompany == null)
            {
                return StatusCode(500, "An error occurred while updating the company.");
            }

            return Ok(updatedCompany);
        }


        // GET: /api/Companies/{id}/contacts
        [HttpGet("{id}/contacts")]
        public async Task<IActionResult> GetContactsByCompanyId(int id)
        {
            var contacts = await contactsRepo.GetContactsByCompanyIdAsync(id);
            return Ok(contacts);
        }

        // GET: /api/Companies/{id}/contacts/{contactId}
        [HttpGet("{id}/contacts/{contactId}")]
        public async Task<IActionResult> GetContactById(int id, int contactId)
        {
            var contact = await contactsRepo.GetContactByIdAsync(contactId);
            if (contact == null || contact.CompanyId != id || contact.IsDeleted)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        // POST: /api/Companies/{id}/contacts
        [HttpPost("{id}/contacts")]
        public async Task<IActionResult> InsertContact(int id, [FromBody] CompanyContact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            contact.CompanyId = id;
            contact.Created = DateTime.UtcNow;
            contact.Modified = DateTime.UtcNow;
            contact.CreatedBy = "Conor";
            contact.ModifiedBy = "Conor";
            contact.IsDeleted = false;

            var createdContact = await contactsRepo.InsertContactAsync(contact);

            return CreatedAtAction(nameof(GetContactById), new { id = createdContact.CompanyId, contactId = createdContact.Id }, createdContact);
        }


        // PUT: /api/Companies/{id}/contacts/{contactId}
        [HttpPut("{id}/contacts/{contactId}")]
        public async Task<IActionResult> UpdateContact(int id, int contactId, [FromBody] CompanyContact contact)
        {
            if (contact.Id != contactId || contact.CompanyId != id)
            {
                return BadRequest("The Ids in the path do not match the Ids in the contact object.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingContact = await contactsRepo.GetContactByIdAsync(contactId);
            if (existingContact == null || existingContact.CompanyId != id || existingContact.IsDeleted)
            {
                return NotFound();
            }

            contact.Created = existingContact.Created;
            contact.CreatedBy = existingContact.CreatedBy;
            contact.Modified = DateTime.UtcNow;
            contact.ModifiedBy = "Kyle";

            var updatedContact = await contactsRepo.UpdateContactAsync(contact);
            if (updatedContact == null)
            {
                return StatusCode(500, "An error occurred while updating the contact.");
            }

            return Ok(updatedContact);
        }


        // DELETE: /api/Companies/{id}/contacts/{contactId}
        [HttpDelete("{id}/contacts/{contactId}")]
        public async Task<IActionResult> DeleteContact(int id, int contactId)
        {
            var contact = await contactsRepo.GetContactByIdAsync(contactId);
            if (contact == null || contact.CompanyId != id || contact.IsDeleted)
            {
                return NotFound();
            }

            contact.Modified = DateTime.UtcNow;
            contact.ModifiedBy = "Kyle";

            await contactsRepo.SoftDeleteContactAsync(contactId);

            return NoContent();
        }

        [HttpGet("reports/company-count-by-state")]
        public async Task<IActionResult> GetCompanyCountByState()
        {
            var result = await repo.GetCompanyCountByStateAsync();
            return Ok(result);
        }

        // New method to list deleted companies
        [HttpGet("recycle-bin")]
        public async Task<IActionResult> GetDeletedCompanies()
        {
            var companies = await repo.GetDeletedCompaniesAsync();
            return Ok(companies);
        }

        // New method to undelete a company
        [HttpPost("recycle-bin/{companyId}/undelete")]
        public async Task<IActionResult> UndeleteCompany(int companyId)
        {
            var success = await repo.UndeleteCompanyAsync(companyId);
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }
    }

}
