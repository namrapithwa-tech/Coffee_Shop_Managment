using Microsoft.AspNetCore.Mvc;
using Static_crud.Models;
using System.Data.SqlClient;
using System.Data;

namespace Static_crud.Controllers
{
    public class OrderController : Controller
    {
        private readonly IConfiguration configuration;

        public OrderController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region GetOrders
        public IActionResult Order()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Order_SelectAll]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return View(table); // Pass the DataTable to the view
                    }
                }
            }
        }
        #endregion

        #region DeleteOrder
        public IActionResult OrderDelete(int OrderID)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Order_Delete]";
                    command.Parameters.Add("@OrderID", SqlDbType.Int).Value = OrderID;
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Order");
        }
        #endregion

        #region AddOrder
        public IActionResult Add_Order(int? OrderID = null)
        {
            OrderModel modelOrder = new OrderModel();
            string connectionString = configuration.GetConnectionString("ConnectionString");

            #region Dropdown
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Customer_DropDown";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        List<CustomerDropDownModel> customerList = new List<CustomerDropDownModel>();
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            CustomerDropDownModel customerDropDownModel = new CustomerDropDownModel
                            {
                                CustomerID = Convert.ToInt32(dataRow["CustomerID"]),
                                CustomerName = dataRow["CustomerName"].ToString()
                            };
                            customerList.Add(customerDropDownModel);
                        }
                        ViewBag.CustomerList = customerList;
                    }
                }
            }
            #endregion

            #region AddEdit
            if (OrderID != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "[dbo].[PR_Order_SelectAll]";
                        command.Parameters.Add("@OrderID", SqlDbType.Int).Value = OrderID;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                modelOrder.OrderID = Convert.ToInt32(reader["OrderID"]);
                                modelOrder.OrderDate = Convert.ToDateTime(reader["OrderDate"]);
                                modelOrder.CustomerID = Convert.ToInt32(reader["CustomerID"]);
                                modelOrder.PaymentMode = reader["PaymentMode"].ToString();
                                modelOrder.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                                modelOrder.ShippingAddress = reader["ShippingAddress"].ToString();
                                modelOrder.UserID = Convert.ToInt32(reader["UserID"]);
                            }
                        }
                    }
                }
            }
            return View(modelOrder);
            #endregion
        }
        #endregion


        #region Save
        [HttpPost]
        public IActionResult Save(OrderModel modelOrder)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (modelOrder.OrderID == null)
                    {
                        command.CommandText = "[dbo].[PR_Order_Insert]";
                    }
                    else
                    {
                        command.CommandText = "[dbo].[PR_Order_Update]";
                        command.Parameters.Add("@OrderID", SqlDbType.Int).Value = modelOrder.OrderID;
                    }

                    command.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = modelOrder.OrderDate;
                    command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = modelOrder.CustomerID;
                    command.Parameters.Add("@PaymentMode", SqlDbType.VarChar).Value = modelOrder.PaymentMode;
                    command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelOrder.TotalAmount;
                    command.Parameters.Add("@ShippingAddress", SqlDbType.VarChar).Value = modelOrder.ShippingAddress;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelOrder.UserID;

                    int result = command.ExecuteNonQuery();

                    if (command.ExecuteNonQuery() > 0)
                    {
                        TempData["OrderInsertMsg"] = modelOrder.OrderID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
                    }
                }
            }
            return RedirectToAction("Order");
        }
        #endregion
    }
}