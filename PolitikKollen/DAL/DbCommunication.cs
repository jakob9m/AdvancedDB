﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;

namespace DAL
{
    public class DbCommunication
    {


        public int AddCounty(string county)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspAddCounty", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CountyName", county);

                // Add return parameter
                SqlParameter returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnParameter);

                connection.Open();
                command.ExecuteNonQuery();

                // Capture the return value from the stored procedure
                int returnValue = (int)returnParameter.Value;
                // Optional: Handle the return value here, or you can handle it where this method is called
                return returnValue;
            }
        }

        public DataTable GetAllCounties()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspGetAllCounties", connection);
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // Load the data into the DataTable
                dt.Load(reader);
                reader.Close();
            }

            return dt;
        }

        public int DeleteCounty(string countyName)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspDeleteCounty", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add the CountyName parameter to the command
                command.Parameters.AddWithValue("@CountyName", countyName);

                // Add a return parameter
                SqlParameter returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnParameter);

                connection.Open();
                command.ExecuteNonQuery();

                // Get the return value
                int result = (int)command.Parameters["@ReturnVal"].Value;

                return result;
            }
        }

        public DataTable GetAllProposals()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspGetProposalPrimaryKeys", connection);
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // Load the data into the DataTable
                dt.Load(reader);
                reader.Close();
            }

            return dt;
        }

        public int CreateProposal(string countyName, string proposal, string info)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspCreateProposal", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add the parameters
                command.Parameters.AddWithValue("@CountyName", countyName);
                command.Parameters.AddWithValue("@Proposal", proposal);
                command.Parameters.AddWithValue("@Info", info);

                // Output parameter for @Result
                SqlParameter resultOutputParameter = new SqlParameter("@Result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(resultOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameter's value
                if (resultOutputParameter.Value != DBNull.Value)
                {
                    return (int)resultOutputParameter.Value;
                }

                // Handle the case where the output parameter is DBNull.Value (null)
                return -1; // You can choose an appropriate error code or value here
            }
        }


        public int DeleteProposal(string countyName, string proposal)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspDeleteProposal", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add the parameters
                command.Parameters.AddWithValue("@CountyName", countyName);
                command.Parameters.AddWithValue("@Proposal", proposal);

                connection.Open();

                // Since you're deleting data, you can use ExecuteNonQuery which returns the number of rows affected.
                // If the deletion is successful, it should return 1. Otherwise, it might return 0.
                return command.ExecuteNonQuery();
            }
        }

        public int EditProposal(string countyName, string proposal, string newInfo)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspEditProposal", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@CountyName", countyName);
                command.Parameters.AddWithValue("@Proposal", proposal);
                command.Parameters.AddWithValue("@NewInfo", newInfo);

                // Add return parameter
                SqlParameter returnParameter = new SqlParameter("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnParameter);

                connection.Open();
                command.ExecuteNonQuery();

                // Capture the return value from the stored procedure
                int returnValue = (int)returnParameter.Value;
                // Optional: Handle the return value here, or you can handle it where this method is called
                return returnValue;
            }
        }

        public byte[] GetSaltByUserName(string userName, String type)
        {
            byte[] salt = null; // Initialize to null. Will store the salt value if found.

            String call = "";
            String Value = "";

            if (type.Equals("User"))
            {
                call = "pk.uspGetSaltByUserName";
                Value = "@UserName";
            }
            else if (type.Equals("Admin"))
            {
                call = "pk.uspGetSaltByAdminName";
                Value = "@AdminName";
            }

            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection handling logic
            {
                SqlCommand command = new SqlCommand(call, connection);
                command.CommandType = CommandType.StoredProcedure;

                // Input parameter
                command.Parameters.AddWithValue(Value, userName);

                // Output parameter
                SqlParameter saltOutputParameter = new SqlParameter("@Salt", SqlDbType.VarBinary, 64);
                saltOutputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(saltOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameter value
                if (saltOutputParameter.Value != DBNull.Value)
                {
                    salt = (byte[])saltOutputParameter.Value;
                }
            }

            return salt; // This will be null if no salt was found for the given UserName
        }




        public bool CheckCitizenExistence(byte[] bankIdHash, String type, out string message)
        {
            bool exists = false; // This flag will be used to store the existence status
            message = ""; // Initialize the message string

            String call = "";

            if (type.Equals("User"))
            {
                call = "pk.uspCheckCitizenExistence";
            }
            else if (type.Equals("Admin"))
            {
                call = "pk.uspCheckAdminExistence";
            }

            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection logic
            {
                SqlCommand command = new SqlCommand(call, connection);
                command.CommandType = CommandType.StoredProcedure;

                // Input parameter for BankIdHash
                command.Parameters.AddWithValue("@BankIdHash", bankIdHash);

                // Output parameters for Exists and Message
                SqlParameter existsOutputParameter = new SqlParameter("@Exists", SqlDbType.Bit);
                existsOutputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(existsOutputParameter);

                SqlParameter messageOutputParameter = new SqlParameter("@Message", SqlDbType.NVarChar, 255);
                messageOutputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(messageOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameter values
                if (existsOutputParameter.Value != DBNull.Value)
                {
                    exists = Convert.ToBoolean(existsOutputParameter.Value);
                }

                if (messageOutputParameter.Value != DBNull.Value)
                {
                    message = Convert.ToString(messageOutputParameter.Value);
                }
            }

            return exists; // This will be true if BankIdHash exists, false otherwise
        }

        public DataTable GetProposalDataAsDataTable(byte[] bankIdHash, string countyName)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection logic
            {
                using (SqlCommand command = new SqlCommand("pk.uspGetProposalData", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    command.Parameters.AddWithValue("@BankIdHash", bankIdHash);
                    command.Parameters.AddWithValue("@CountyName", countyName);

                    // Open the connection
                    connection.Open();

                    // Use SqlDataAdapter to fill DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        public string GetCountyByBankIdHash(byte[] bankIdHash)
        {
            string countyName = null; // Initialize to null. Will store the county name if found.

            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection handling logic
            {
                SqlCommand command = new SqlCommand("pk.uspGetCountyByBankIdHash", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Input parameter
                command.Parameters.AddWithValue("@BankIdHash", bankIdHash);

                // Output parameter
                SqlParameter countyNameOutputParameter = new SqlParameter("@CountyName", SqlDbType.VarChar, 255);
                countyNameOutputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(countyNameOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameter value
                if (countyNameOutputParameter.Value != DBNull.Value)
                {
                    countyName = countyNameOutputParameter.Value.ToString();
                }
            }

            return countyName; // This will be null if no county was found for the given BankIdHash
        }

        public void CreateUser(byte[] bankIdHash, string userName, byte[] salt, string county)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection handling logic
            {
                SqlCommand command = new SqlCommand("pk.uspCreateUser", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@BankIdHash", bankIdHash);
                command.Parameters.AddWithValue("@Salt", salt);
                command.Parameters.AddWithValue("@UserName", userName);

                if (county != null)
                {
                    command.Parameters.AddWithValue("@County", county);
                }
                else
                {
                    command.Parameters.AddWithValue("@County", DBNull.Value);
                }

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();
            }
        }

        public void CreateAdmin(byte[] bankIdHash, string AdminName, byte[] salt)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection()) // Replace with your actual connection handling logic
            {
                SqlCommand command = new SqlCommand("pk.uspCreateAdmin", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@BankIdHash", bankIdHash);
                command.Parameters.AddWithValue("@Salt", salt);
                command.Parameters.AddWithValue("@AdminName", AdminName);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();
            }
        }


        public (string CitizenName, string CountyName) GetCitizenDataByUserHash(byte[] userHash)
        {
            string citizenName = null;
            string countyName = null; // Initialize countyName at the start of the method

            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspGetUserDetailsByHash", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Input parameter
                command.Parameters.Add(new SqlParameter("@UserHash", SqlDbType.VarBinary, 64)).Value = userHash;

                // Output parameters
                SqlParameter nameOutputParameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(nameOutputParameter);

                SqlParameter countyNameOutputParameter = new SqlParameter("@County", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(countyNameOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameters' values
                if (nameOutputParameter.Value != DBNull.Value)
                {
                    citizenName = (string)nameOutputParameter.Value;
                }
                if (countyNameOutputParameter.Value != DBNull.Value)
                {
                    countyName = (string)countyNameOutputParameter.Value;
                }
            }

            return (citizenName, countyName);
        }

        public string GetAdminDataByUserHash(byte[] userHash)
        {
            string adminName = null;

            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspGetAdminDetailsByHash", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Input parameter
                SqlParameter userHashParameter = new SqlParameter("@UserHash", SqlDbType.VarBinary, 64)
                {
                    Value = userHash
                };
                command.Parameters.Add(userHashParameter);

                // Output parameter for Admin Name
                SqlParameter nameOutputParameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(nameOutputParameter);

                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Retrieve the output parameter's value
                if (nameOutputParameter.Value != DBNull.Value)
                {
                    adminName = (string)nameOutputParameter.Value;
                }
            }

            return adminName;
        }


        public int deleteUser(byte[] id)
        {
            int result = 0;

            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspDeleteCitizen", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add the parameters
                command.Parameters.AddWithValue("@BankIdHash", id);

                // Add output parameter
                SqlParameter outputParam = new SqlParameter("@Result", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(outputParam);

                connection.Open();
                command.ExecuteNonQuery();

                // Retrieve the value of the output parameter
                result = (int)command.Parameters["@Result"].Value;
            }

            return result;
        }


        public int UpdateCountyNameByBankIDHash(byte[] bankIDHash, string newCountyName)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("uspUpdateCountyNameByBankIDHash", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@BankIDHash", bankIDHash);
                command.Parameters.AddWithValue("@NewCountyName", newCountyName);

                // Add return parameter
                SqlParameter returnParameter = new SqlParameter("@Result", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(returnParameter);

                connection.Open();
                command.ExecuteNonQuery();

                // Capture the return value from the stored procedure
                int returnValue = (int)returnParameter.Value;

                return returnValue;
            }
        }

        public int SaveOpinion(byte[] bankIDHash, string proposal, string countyName, int voteFor, int voteAgainst)
        {
            using (SqlConnection connection = ConnectionHandler.GetConnection())
            {
                SqlCommand command = new SqlCommand("pk.uspSaveOpinion", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@BankIdHash", bankIDHash);
                command.Parameters.AddWithValue("@Proposal", proposal);
                command.Parameters.AddWithValue("@CountyName", countyName);
                command.Parameters.AddWithValue("@VoteFor", voteFor);
                command.Parameters.AddWithValue("@VoteAgainst", voteAgainst);

                
                // Add return parameter
                SqlParameter returnParameter = new SqlParameter("@Result", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnParameter);

                System.Diagnostics.Debug.WriteLine(BitConverter.ToString(bankIDHash).Replace("-", ""));
                System.Diagnostics.Debug.WriteLine(proposal);
                System.Diagnostics.Debug.WriteLine(countyName);
                System.Diagnostics.Debug.WriteLine(voteFor);
                System.Diagnostics.Debug.WriteLine(voteAgainst);

                connection.Open();
                command.ExecuteNonQuery();

                // Capture the return value from the stored procedure
                int returnValue = (int)returnParameter.Value;

                return returnValue;
            }
        }




    }
}
