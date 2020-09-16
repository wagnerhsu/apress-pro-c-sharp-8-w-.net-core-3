﻿using System;
using System.Collections.Generic;
using System.Data;
using AutoLot.DAL.Models;
using Microsoft.Data.SqlClient;

namespace AutoLot.DAL.DataOperations
{
    public class InventoryDAL : IDisposable
    {
        private SqlConnection _sqlConnection = null;
        private readonly string _connectionString;

        public InventoryDAL() : this(
            @"Data Source=.,5433;User Id=sa;Password=P@ssw0rd;Initial Catalog=AutoLot")
        {
        }

        public InventoryDAL(string connectionString)
            => _connectionString = connectionString;

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _sqlConnection.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection
            {
                ConnectionString = _connectionString
            };
            _sqlConnection.Open();
        }

        private void CloseConnection()
        {
            if (_sqlConnection?.State != ConnectionState.Closed)
            {
                _sqlConnection?.Close();
            }
        }

        public List<Car> GetAllInventory()
        {
            OpenConnection();
            // This will hold the records.
            List<Car> inventory = new List<Car>();

            // Prep command object.
            string sql = "Select * From Inventory";
            using SqlCommand command = new SqlCommand(sql, _sqlConnection)
            {
                CommandType = CommandType.Text
            };
            SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                inventory.Add(new Car
                {
                    Id = (int) dataReader["Id"],
                    Color = (string) dataReader["Color"],
                    Make = (string) dataReader["Make"],
                    PetName = (string) dataReader["PetName"]
                });
            }

            dataReader.Close();
            return inventory;
        }

        public Car GetCar(int id)
        {
            OpenConnection();
            Car car = null;
            //This should use parameters for security reasons
            //string sql = $"Select * From Inventory where Id = {id}";
            //using SqlCommand command = new SqlCommand(sql, _sqlConnection)
            //{
            //    CommandType = CommandType.Text
            //};
            SqlParameter param = new SqlParameter
            {
                ParameterName = "@carId",
                Value = id,
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            };
            string sql = $"Select * From Inventory where Id = @carId";
            using SqlCommand command = new SqlCommand(sql, _sqlConnection)
            {
                CommandType = CommandType.Text
            };
            command.Parameters.Add(param);
            SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                car = new Car
                {
                    Id = (int) dataReader["Id"],
                    Color = (string) dataReader["Color"],
                    Make = (string) dataReader["Make"],
                    PetName = (string) dataReader["PetName"]
                };
            }

            dataReader.Close();
            return car;
        }

        public void InsertAutoOld(string color, string make, string petName)
        {
            OpenConnection();
            // Format and execute SQL statement.
            string sql = $"Insert Into Inventory (Make, Color, PetName) Values ('{make}', '{color}', '{petName}')";

            // Execute using our connection.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }

            CloseConnection();
        }

        public void InsertAutoNoParams(Car car)
        {
            OpenConnection();
            // Format and execute SQL statement.
            string sql = "Insert Into Inventory (Make, Color, PetName) Values " +
                         $"('{car.Make}', '{car.Color}', '{car.PetName}')";

            // Execute using our connection.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }

            CloseConnection();
        }

        public void InsertAuto(Car car)
        {
            OpenConnection();
            // Note the "placeholders" in the SQL query.
            string sql = "Insert Into Inventory" +
                         "(Make, Color, PetName) Values" +
                         "(@Make, @Color, @PetName)";

            // This command will have internal parameters.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                // Fill params collection.
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@Make",
                    Value = car.Make,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@Color",
                    Value = car.Color,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@PetName",
                    Value = car.PetName,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);

                command.ExecuteNonQuery();
                CloseConnection();
            }
        }

        public void DeleteCar(int id)
        {
            OpenConnection();
            // Get ID of car to delete, then do so.
            //string sql = $"Delete from Inventory where Id = '{id}'";
            SqlParameter param = new SqlParameter
            {
                ParameterName = "@carId",
                Value = id,
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            };
            string sql = "Delete from Inventory where Id = @carId";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.Parameters.Add(param);
                try
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! That car is on order!", ex);
                    throw error;
                }
            }

            CloseConnection();
        }

        public void UpdateCarPetName(int id, string newPetName)
        {
            OpenConnection();
            // Get ID of car to modify the pet name.
            SqlParameter paramId = new SqlParameter
            {
                ParameterName = "@carId",
                Value = id,
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            };
            SqlParameter paramName = new SqlParameter
            {
                ParameterName = "@petName",
                Value = newPetName,
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Input
            };
            string sql = $"Update Inventory Set PetName = '{newPetName}' Where Id = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.Parameters.Add(paramId);
                command.Parameters.Add(paramName);
                command.ExecuteNonQuery();
            }

            CloseConnection();
        }

        public string LookUpPetName(int carId)
        {
            OpenConnection();
            string carPetName;

            // Establish name of stored proc.
            using (SqlCommand command = new SqlCommand("GetPetName", _sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Input param.
                SqlParameter param = new SqlParameter
                {
                    ParameterName = "@carId",
                    SqlDbType = SqlDbType.Int,
                    Value = carId,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(param);

                // Output param.
                param = new SqlParameter
                {
                    ParameterName = "@petName",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(param);

                // Execute the stored proc.
                command.ExecuteNonQuery();

                // Return output param.
                carPetName = (string) command.Parameters["@petName"].Value;
                CloseConnection();
            }

            return carPetName;
        }

        public void ProcessCreditRisk(bool throwEx, int customerId)
        {
            OpenConnection();
// First, look up current name based on customer ID.
            string fName;
            string lName;
            var cmdSelect =
                new SqlCommand("Select * from Customers where Id = @customerId",
                    _sqlConnection);
            SqlParameter paramId = new SqlParameter
            {
                ParameterName = "@customerId",
                SqlDbType = SqlDbType.Int,
                Value = customerId,
                Direction = ParameterDirection.Input
            };
            cmdSelect.Parameters.Add(paramId);
            using (var dataReader = cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string) dataReader["FirstName"];
                    lName = (string) dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }
            }

            cmdSelect.Parameters.Clear();
// Create command objects that represent each step of the operation.
            var cmdRemove =
                new SqlCommand("Delete from Customers where Id = @customerId",
                    _sqlConnection);
            cmdRemove.Parameters.Add(paramId);
            var cmdInsert =
                new SqlCommand(
                    "Insert Into CreditRisks (CustomerId,FirstName, LastName) Values(@customerId,@FirstName, @LastName)",
                    _sqlConnection);
            SqlParameter parameterId2 = new SqlParameter
            {
                ParameterName = "@CustomerId",
                SqlDbType = SqlDbType.Int,
                Value = customerId,
                Direction = ParameterDirection.Input
            };
            SqlParameter parameterFirstName = new SqlParameter
            {
                ParameterName = "@FirstName",
                Value = fName,
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Input
            };

            SqlParameter parameterLastName = new SqlParameter
            {
                ParameterName = "@LastName",
                Value = lName,
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Input
            };

            cmdInsert.Parameters.Add(parameterId2);
            cmdInsert.Parameters.Add(parameterFirstName);
            cmdInsert.Parameters.Add(parameterLastName);
// We will get this from the connection object.
            SqlTransaction tx = null;
            try
            {
                tx = _sqlConnection.BeginTransaction();

                // Enlist the commands into this transaction.
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;

                // Execute the commands.
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                // Simulate error.
                if (throwEx)
                {
                    throw new Exception("Sorry!  Database error! Tx failed...");
                }

                // Commit it!
                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Any error will roll back transaction.  Using the new conditional access operator to check for null.
                tx?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}