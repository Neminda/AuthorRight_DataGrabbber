using AuthorRightClaim.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class Utilities
{
    /// <summary>
    /// Read the external config file value using the given key
    /// </summary>
    /// <returns></returns>
    public static string ReadConfigFile(string configKey)
    {
        try
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            //ConfigFilePath -> Key of the file path in App.config
            fileMap.ExeConfigFilename = ConfigurationManager.AppSettings["Configs"];
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            ConfigurationSection configSec = config.GetSection("appSettings");
            //configKey -> Passed key for read a value from the external config file
            KeyValueConfigurationElement key = ((AppSettingsSection)(configSec)).Settings[configKey];

            if (key.Value == null)
            {
                Console.WriteLine("Error reading Configuration file!");
                return null;
            }
            else
            {
                string valueString = key.Value;
                return valueString.Trim();
            }
        }
        catch (Exception ex)
        {           
            sendEmail("Error reading Configuration file!, Error - " + ex);
            return null;
        }
    }

    /// <summary>
    /// Read DB config file and get db url
    /// </summary>
    /// <param name="configKey"></param>
    /// <returns></returns>
    public static string ReadDBConfig(string configKey)
    {
        try
        {
            //DBConfig -> Key of the file path in App.config
            string filePath = ConfigurationManager.AppSettings["Configs"];
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            //DataBase.config -> External config file name
            fileMap.ExeConfigFilename = filePath + "DataBase.config";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            ConfigurationSection configSec = config.GetSection("connectionStrings");
            //configKey -> Passed key for read a value from the external config file
            string connString = ((ConnectionStringsSection)configSec).ConnectionStrings["Euro"].ConnectionString;

            if (connString == null)
            {
                return null;
            }
            else
            {
                string valueString = connString;
                return valueString.Trim();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Get table schema from DB
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static DataTable GetDbTableSchema(string tableName)
    {
        string sql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
        string connString = ReadConfigFile("TvGuide");
        DataTable dbSchemaTbl = DBConnect.GetdataTable(connString, sql);
        dbSchemaTbl.Columns.Add("data", typeof(string));
        return dbSchemaTbl;
    }

    /// <summary>
    /// Executes normal sql queries
    /// </summary>
    /// <param name="sqlString"></param>
    public static void ExecuteSql(string connectString, string sqlString)
    {
        try
        {
            SqlConnection conn = null;
            conn = new SqlConnection(connectString);
            if (conn != null)
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                //GetConnection(connectString);
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = conn;
                sCmd.CommandType = CommandType.Text;
                sCmd.CommandText = sqlString;

                SqlDataAdapter sqlDAdptr = new SqlDataAdapter();
                //Retrieves data from the data source
                sqlDAdptr.SelectCommand = sCmd;
                sCmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
            }
            else
            {
                Console.WriteLine("Error in DB Connection");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            //if (conn.State != ConnectionState.Closed)
            //conn.Close();
        }
    }

    /// <summary>
    /// Remove special characters
    /// </summary>
    /// <param name="beforeString"></param>
    /// <returns></returns>
    public static string removeSpecialCharachters(string beforeString)
    {
        if (string.IsNullOrWhiteSpace(beforeString))
        {
            return null;
        }
        else
        {
            return Regex.Replace(beforeString, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
    }

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="senderMailAddress"></param>
    /// <param name="mailSubject"></param>
    public static void sendEmail(string mailBody)
    {
        try
        {
            //Print message in console
            Console.WriteLine(mailBody);

            string email = ReadConfigFile("Email");
            MailMessage mailMsg = new MailMessage();
            MailAddress mailAddress = new MailAddress(email);
            //From address
            mailMsg.From = mailAddress;

            //Construct mail
            mailMsg.Subject = "Error occurred in Author Right";
            mailMsg.Body = mailBody + Environment.NewLine + Environment.NewLine + "Author Rights.";
            mailMsg.To.Add(ReadConfigFile("AdminEmail"));

            SmtpClient client = new SmtpClient();
            client.Port = Convert.ToInt32(ReadConfigFile("SMTP_Port"));
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, ReadConfigFile("PWD"));
            client.EnableSsl = true;
            client.Host = ReadConfigFile("SMTP_Host");
            //Send
            client.Send(mailMsg);

            client.ServicePoint.CloseConnectionGroup(client.ServicePoint.ConnectionName);
            client.Dispose();
            client = null;
            mailMsg.Dispose();
            mailMsg = null;          
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occurred while sending error email " + ex);
        }
    }
}

