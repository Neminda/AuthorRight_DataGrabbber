using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorRightClaim.Classes
{
    class PopulateDB
    {
        //public static Dictionary<string, string> data = new Dictionary<string, string>();

            public static string conn { get; set; }

        /// <summary>
        /// Update DB with channel data
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="dataDict"></param>
        /// <returns></returns>
        public static bool updateDB(Dictionary<string, string> dataDict)
        {
            try
            {
                string tableName = dataDict["Table"];
                DataTable schema = Utilities.GetDbTableSchema(tableName);
                bool success = true;
                //bool programSuccess = false;
                switch (tableName.ToUpper())
                {
                    case "CHANNEL":
                        readDatatable(dataDict, schema);
                        success = insertChannel(schema);
                        break;

                    case "PROGRAMME":
                        string channel = dataDict["channel"];
                        //readDatatable(dataDict, schema);
                        success = insertProgrm(dataDict, channel);
                        break;

                    default:
                        Utilities.sendEmail("Unidentified Table name");
                        success = false;
                        break;
                }
                schema = null;

                if (success)
                {
                    return true;
                }
                else
                    return false;

            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception occurred during DB update: " + e);
                return false;
            }
        }

        /// <summary>
        /// Read xml data in datatable
        /// </summary>
        /// <param name="dataDict"></param>
        /// <param name="schema"></param>
        private static void readDatatable(Dictionary<string, string> dataDict, DataTable schema)
        {
            foreach (var item in dataDict)
            {
                for (int i = 1; i < schema.Rows.Count; i++)
                {
                    string dbColumnName = schema.Rows[i][0].ToString();
                    if (item.Key.ToUpper() == dbColumnName.ToUpper())
                    {
                        schema.Rows[i]["data"] = item.Value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Insert channel data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="disName"></param>
        /// <param name="language"></param>
        /// <param name="iconSrc"></param>
        /// <param name="url"></param>
        private static bool insertChannel(DataTable data)
        {
            try
            {
                bool success = true;
                conn = Utilities.ReadConfigFile("TvGuide");
                //StringBuilder channelStr = new StringBuilder();
                //channelStr.Append("INSERT INTO Channel ([Id], [display-name], [lang], [src], [url]) VALUES(");

                SqlConnection sqlcon = new SqlConnection(conn);
                sqlcon.Open();
                SqlCommand chnlCommand = new SqlCommand("SP_AUTHORRIGHT_InsertChannelData", sqlcon);
                chnlCommand.CommandType = CommandType.StoredProcedure;

                string key = "";
                string value = "";

                for (int i = 1; i < data.Rows.Count; i++)
                {
                    key = data.Rows[i][0].ToString();
                    value = data.Rows[i][1].ToString();
                    if (key == "cId")
                    {
                        continue;
                    }
                    chnlCommand.Parameters.AddWithValue(key, value);
                    //channelStr.Append("'" + data.Rows[i][1].ToString() + "'");
                    /* if (i == data.Rows.Count - 1)
                     {
                         break;
                     }
                     else
                         channelStr.Append(",");*/
                }
                //channelStr.Append(")");
                //Utilities.ExecuteSql(conn, channelStr.ToString());
                chnlCommand.ExecuteNonQuery();
                chnlCommand.Parameters.Clear();
                return success;
            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception occurred during channel data insert: " + e);
                return false;
            }
        }

        /// <summary>
        /// Insert program data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="disName"></param>
        /// <param name="language"></param>
        /// <param name="iconSrc"></param>
        /// <param name="url"></param>
        private static bool insertProgrm(Dictionary<string, string> programData, string channelName)
        {
            try
            {
                bool success = true;
                Dictionary<string, string> dict = new Dictionary<string, string>();
                bool haveCredits = false;
                conn = Utilities.ReadConfigFile("TvGuide");
                StringBuilder programStr = new StringBuilder();

                string value = null;
                string key = null;
                string prgTitle = null;
                string prgStart = null;

                SqlConnection sqlcon = new SqlConnection(conn);
                sqlcon.Open();
                SqlCommand prgmCommand = new SqlCommand("SP_AUTHORRIGHT_InsertProgramData", sqlcon);
                prgmCommand.CommandType = CommandType.StoredProcedure;

                foreach (var item in programData)
                {
                    key = item.Key;
                    //key = key.Replace("-", "");
                    value = item.Value;

                    //Get title and start to get programme id
                    if (key == "title")
                        prgTitle = value;

                    if (key == "start")
                        prgStart = value;

                    //Check for credits
                    if (key.Contains("director") ||
                        key.Contains("actor") || 
                        key.Contains("writer") || 
                        key.Contains("adapter") || 
                        key.Contains("producer") || 
                        key.Contains("composer") || 
                        key.Contains("editor") || 
                        key.Contains("presenter") || 
                        key.Contains("commentator") || 
                        key.Contains("guest"))
                    {
                        haveCredits = true;
                        dict.Add(key, value);
                    }

                    DataTable programSchema = Utilities.GetDbTableSchema("programme");
                    string dbColumnName = "";
                    for (int i = 1; i < programSchema.Rows.Count; i++)
                    {
                        dbColumnName = programSchema.Rows[i][0].ToString();

                        if (dbColumnName.ToUpper().Contains(item.Key.ToUpper()))
                        {
                            if (!prgmCommand.Parameters.Contains(key))
                            {
                                if (value == "")
                                    prgmCommand.Parameters.AddWithValue(key, null);

                                else
                                    prgmCommand.Parameters.AddWithValue(key, value);
                            }
                        }
                    }
                }
                prgmCommand.Parameters.AddWithValue("@channel", channelName);
                prgmCommand.ExecuteNonQuery();
                prgmCommand.Parameters.Clear();
                if (haveCredits)
                {
                    success = insertCredit(conn, dict, prgTitle, prgStart);
                }
                sqlcon.Close();
                dict.Clear();

                if (!success)
                {
                    return false;
                }
                else
                    return success;
            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception occurred during programme data insert: " + e);
                return false;
            }
        }

        /// <summary>
        /// Insert channel data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="disName"></param>
        /// <param name="language"></param>
        /// <param name="iconSrc"></param>
        /// <param name="url"></param>
        private static bool insertCredit(string connString, Dictionary<string, string> creditData,/*string creditType,string creditName,*/string prgmTitle, string prgmStart)
        {
            try
            {
                bool programSuccess = true;
                SqlConnection sqlcon = new SqlConnection(connString);
                sqlcon.Open();
                SqlCommand creditCommand = new SqlCommand("SP_AUTHORRIGHT_InsertCreditData", sqlcon);
                creditCommand.CommandType = CommandType.StoredProcedure;
                foreach (var credit in creditData)
                {
                    string cKey = credit.Key;
                    if (cKey.Contains("_"))
                    {
                        string[] creditKey = cKey.Split('_');
                        cKey = creditKey[1];
                    }
                    creditCommand.Parameters.AddWithValue("@type", cKey);
                    creditCommand.Parameters.AddWithValue("@name", credit.Value);
                    creditCommand.Parameters.AddWithValue("@programTitle", prgmTitle);
                    creditCommand.Parameters.AddWithValue("@start", prgmStart);
                    creditCommand.ExecuteNonQuery();
                    creditCommand.Parameters.Clear();
                }

                sqlcon.Close();
                return programSuccess;
                //string sql = $"EXEC SP_AUTHORRIGHT_InsertCreditData {type, name, prgmTitle, prgmStart}";
                //Utilities.ExecuteSql(sqlcon, creditStr.ToString());
            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception occurred during credit data insert: " + e);
                return false;
            }
        }

    }
}
