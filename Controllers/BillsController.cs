using Microsoft.AspNetCore.Mvc;
using Static_crud.Models;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;

namespace Static_crud.Controllers
{
    public class BillsController : Controller
    {
        private IConfiguration configuration;

        public BillsController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        #region Getall
        public IActionResult Bills()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Bills_SelectAll]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }
        #endregion

        #region Delete
        public IActionResult DeleteBill(int BillID)
        {


            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_Bills_Delete]";
            command.Parameters.Add("@BillID", SqlDbType.Int).Value = BillID;
            command.ExecuteNonQuery();
            return RedirectToAction("Bills");

        }
        #endregion

        #region AddEdit
        public IActionResult AddBills(int? BillID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            #region DropDownOrderID
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_Order_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();
            foreach (DataRow dataRow in dataTable1.Rows)
            {
                OrderDropDownModel orderDropDownModel = new OrderDropDownModel();
                orderDropDownModel.OrderID = Convert.ToInt32(dataRow["OrderID"]);
                orderList.Add(orderDropDownModel);
            }
            ViewBag.orderList = orderList;

            #endregion

            #region AddEdit

            BillsModel modelBills = new BillsModel();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_Bills_SelectByPK]";
                    command.Parameters.Add("@BillID", SqlDbType.Int).Value = (object)BillID ?? DBNull.Value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && BillID != null)
                        {
                            reader.Read();
                            modelBills.BillID = Convert.ToInt32(reader["BillID"]);
                            modelBills.BillNumber = reader["BillNumber"].ToString();
                            modelBills.BillDate = Convert.ToDateTime(reader["BillDate"]);
                            modelBills.OrderID = Convert.ToInt32(reader["OrderID"]);
                            modelBills.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                            modelBills.Discount = Convert.ToDecimal(reader["Discount"]);
                            modelBills.NetAmount = Convert.ToDecimal(reader["NetAmount"]);
                            modelBills.UserID = Convert.ToInt32(reader["UserID"]);
                        }
                    }
                }
            }

            return View(modelBills);


            #endregion

        }
        #endregion

        #region Save
        [HttpPost]
        public IActionResult Save(BillsModel modelBills)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if (modelBills.BillID == null)
            {
                command.CommandText = "[dbo].[PR_Bills_Insert]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_Bills_Update]";
                command.Parameters.Add("@BillID", SqlDbType.Int).Value = modelBills.BillID;
            }

            command.Parameters.Add("@BillNumber", SqlDbType.VarChar).Value = modelBills.BillNumber;
            command.Parameters.Add("@BillDate", SqlDbType.DateTime).Value = modelBills.BillDate;
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = modelBills.OrderID;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = modelBills.TotalAmount;
            command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = modelBills.Discount;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = modelBills.NetAmount;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelBills.UserID;

            if (command.ExecuteNonQuery() > 0)
            {
                TempData["BillInsertMsg"] = modelBills.BillID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
            }

            connection.Close();
            return RedirectToAction("Bills");
        }
        #endregion


    }
}
