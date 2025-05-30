using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;

namespace Practice.DataAccess.Helper
{
    public static class DbConnectionHelper
    {
        public static string GetConnectionString()
        {
            return @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\Дмитрий\Documents\GitHub\Practice\Practice.DataAccess\DB\БД.accdb;";
        }
    }
}
