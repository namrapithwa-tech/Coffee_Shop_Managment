using Microsoft.AspNetCore.Mvc;
using Static_crud.Models;
using System.Data.SqlClient;
using System.Data;

namespace Static_crud.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly IConfiguration configuration;

        public OrderDetailController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region GetAll
        public IActionResult OrderDetail()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Order_Details_SelectAll]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return View(table);
                    }
                }
            }
        }
        #endregion

        #region Delete
        public IActionResult Delete_OrderDetail(int OrderDetailID)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Order_Details_Delete]";
                    command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = OrderDetailID;
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("OrderDetail");
        }
        #endregion

        #region PopulateDropDowns
        private void PopulateDropDowns()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");

            List<ProductDropDownModel> productList = new List<ProductDropDownModel>();
            List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();

            #region DropdownProductID
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PR_Product_DropDown", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        foreach (DataRow row in dataTable.Rows)
                        {
                            ProductDropDownModel productDropDownModel = new ProductDropDownModel
                            {
                                ProductID = Convert.ToInt32(row["ProductID"]),
                                ProductName = row["ProductName"].ToString()
                            };
                            productList.Add(productDropDownModel);
                        }
                    }
                }
            }
            #endregion

            #region DropdownOrderID
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PR_Order_DropDown", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        foreach (DataRow row in dataTable.Rows)
                        {
                            OrderDropDownModel orderDropDownModel = new OrderDropDownModel
                            {
                                OrderID = Convert.ToInt32(row["OrderID"]),
                            };
                            orderList.Add(orderDropDownModel);
                        }
                    }
                }
            }
            #endregion

            ViewBag.ProductList = productList;
            ViewBag.OrderList = orderList;

        }
        #endregion

        #region Add or Edit
        public IActionResult Add_OrderDetail(int? OrderDetailID)
        {
            PopulateDropDowns();
            OrderDetailModel orderDetailModel = new OrderDetailModel();

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_OrderDetail_SelectByPK]";
                    command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = (object)OrderDetailID ?? DBNull.Value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && OrderDetailID != null)
                        {
                            reader.Read();
                            orderDetailModel.OrderDetailID = Convert.ToInt32(reader["OrderDetailID"]);
                            orderDetailModel.OrderID = Convert.ToInt32(reader["OrderID"]);
                            orderDetailModel.ProductID = Convert.ToInt32(reader["ProductID"]);
                            orderDetailModel.Quantity = Convert.ToInt32(reader["Quantity"]);
                            orderDetailModel.Amount = Convert.ToDecimal(reader["Amount"]);
                            orderDetailModel.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                            orderDetailModel.UserID = Convert.ToInt32(reader["UserID"]);
                        }
                    }
                }
            }

            return View(orderDetailModel);
        }

        #endregion

        #region Save
        [HttpPost]
        public IActionResult Save(OrderDetailModel modelOrderDetail)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (modelOrderDetail.OrderDetailID != 0 && modelOrderDetail.OrderDetailID != null)
                    {
                        command.CommandText = "[dbo].[PR_Order_Update]";
                        command.Parameters.Add("@OrderDetailID", SqlDbType.Int).Value = modelOrderDetail.OrderDetailID;
                    }
                    else
                    {
                        command.CommandText = "[dbo].[PR_Order_Insert]";
                    }

                    command.Parameters.Add("@OrderID", SqlDbType.Int).Value = modelOrderDetail.OrderID;
                    command.Parameters.Add("@ProductID", SqlDbType.Int).Value = modelOrderDetail.ProductID;
                    command.Parameters.Add("@Quantity", SqlDbType.Int).Value = modelOrderDetail.Quantity;
                    command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = modelOrderDetail.Amount;
                    command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelOrderDetail.TotalAmount;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelOrderDetail.UserID;

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["OrderDetailInsertMsg"] = modelOrderDetail.OrderDetailID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
                    }
                }
            }
            return RedirectToAction("OrderDetail");
        }



        #endregion
    }
}
