using Microsoft.AspNetCore.Mvc;
using Static_crud.Models;
using System.Data.SqlClient;
using System.Data;

namespace Static_crud.Controllers
{
    public class CustomerController : Controller
    {
        private IConfiguration configuration;

        public CustomerController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        #region Get
        public IActionResult Customer()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Customer_SelctAll]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }
        #endregion

        #region Delete
        public IActionResult DeleteCustomer(int CustomerID)
        {


            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Customer_Delete]";
            command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = CustomerID;
            command.ExecuteNonQuery();
            return RedirectToAction("Customer");

        }
        #endregion

        #region AddEdit
        public IActionResult Add_Customer(int? CustomerID)
        {
            CustomerModel modelCustomer= new CustomerModel();

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Customer_SelectByPK]";
                    command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = (object)CustomerID ?? DBNull.Value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && CustomerID != null)
                        {
                            reader.Read();
                            modelCustomer.CustomerID = Convert.ToInt32(reader["CustomerID"]);
                            modelCustomer.CustomerName = reader["CustomerName"].ToString();
                            modelCustomer.HomeAddress = reader["HomeAddress"].ToString();
                            modelCustomer.Email = reader["Email"].ToString();
                            modelCustomer.MobileNo = reader["MobileNo"].ToString();
                            modelCustomer.GST_NO = reader["GST_NO"].ToString();
                            modelCustomer.CityName = reader["CityName"].ToString();
                            modelCustomer.PinCode = reader["PinCode"].ToString();
                            modelCustomer.NetAmount = Convert.ToDecimal(reader["NetAmount"]);
                            modelCustomer.UserID = Convert.ToInt32(reader["UserID"]);
                        }
                    }
                }
            }

            return View(modelCustomer);
        }
        #endregion

        #region Save
        [HttpPost]
        public IActionResult Save(CustomerModel modelCustomer)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelCustomer.CustomerID == null)
            {
                command.CommandText = "[dbo].[PR_Customer_Insert]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_Customer_Update]";
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = modelCustomer.CustomerID;
            }

            command.Parameters.Add("@CustomerName", SqlDbType.VarChar).Value = modelCustomer.CustomerName;
            command.Parameters.Add("@HomeAddress", SqlDbType.VarChar).Value = modelCustomer.HomeAddress;
            command.Parameters.Add("@Email", SqlDbType.VarChar).Value = modelCustomer.Email;
            command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelCustomer.MobileNo;
            command.Parameters.Add("@GST_NO", SqlDbType.VarChar).Value = modelCustomer.GST_NO;
            command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = modelCustomer.CityName;
            command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = modelCustomer.PinCode;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = modelCustomer.NetAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelCustomer.UserID;

            if (command.ExecuteNonQuery() > 0)
            {
                TempData["CustomerInsertMsg"] = modelCustomer.CustomerID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
            }

            connection.Close();
            return RedirectToAction("Customer");
        }
        #endregion
    }
}
