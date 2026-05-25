using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
namespace Server
{
    public static   class SqlConnect
    {
      private static string _connectionString = @"Data Source=(localdb)\lcaimeiclaude;AttachDbFilename=""D:\Work\tada\job\samsung\LCA IMEI\LCA_imei claude\LCA\Database2\Database.mdf"";Integrated Security=True";
        public  static SqlConnection GetConnection
        {
            get {
                return new SqlConnection(_connectionString);
            }
        }
    }
}
