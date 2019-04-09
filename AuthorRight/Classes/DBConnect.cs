using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorRightClaim.Classes
{
    class DBConnect
    {
        /// <summary>
        /// Execute sql queries which returns a datatable 
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns>DataTable</returns>
        public static DataTable GetdataTable(string connectString, string sqlString)
        {
            try
            {
                SqlConnection conn = null;
                conn = new SqlConnection(connectString);
                if (conn != null)
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    DataTable dataTbl = new DataTable();
                    //GetConnection(connectString);
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = conn;
                    sqlCmd.CommandType = CommandType.Text;
                    sqlCmd.CommandText = sqlString;

                    SqlDataAdapter sqlDAdptr = new SqlDataAdapter();
                    //Retrieves data from the data source
                    sqlDAdptr.SelectCommand = sqlCmd;
                    dataTbl = new DataTable();
                    //Fills the dataTable
                    sqlDAdptr.Fill(dataTbl);
                    conn.Close();
                    conn.Dispose();

                    if (dataTbl.Rows.Count <= 0)
                    {
                        dataTbl.Dispose();
                        return null;
                    }
                    else
                    {
                        return dataTbl;
                    }
                }
                else
                {
                    Utilities.sendEmail("Error getting Datatable");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.sendEmail("Exception while retrieving Datatable " + ex);
                throw ex;
            }
            finally
            {
                //if (conn.State != ConnectionState.Closed)
                //conn.Close();
            }
        }
    }
}
