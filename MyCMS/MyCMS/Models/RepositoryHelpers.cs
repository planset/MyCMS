using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCMS.Models
{
    public static class RepositoryHelpers
    {
        public static string GetTableName(Type t)
        {
            return t.Name;
        }
    }
}
