using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;


namespace DapperApp.Repository
{
    public class CompanyCountByState
    {
        public required string State { get; set; }
        public int CompanyCount { get; set; }
    }
}

