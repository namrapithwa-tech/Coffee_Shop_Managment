using Microsoft.AspNetCore.Mvc;
using Static_crud.Models;
using System.Data.SqlClient;
using System.Data;

namespace Static_crud.Controllers
{
    public class UserController : Controller
    {
        private IConfiguration configuration;

        public UserController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        #region GetUSer
        public IActionResult User()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_User_SelectAll]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return View(table);
        }
        #endregion

        #region Delete
        public IActionResult UserDelete(int UserID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_User_Delete]";
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.ExecuteNonQuery();
            return RedirectToAction("User");
        }
        #endregion

        #region Add or Edit
        public IActionResult Add_User(int? UserID)
        {
            UserModel modelUser = new UserModel();

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[PR_User_SelectAll]";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = (object)UserID ?? DBNull.Value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && UserID != null)
                        {
                            reader.Read();
                            modelUser.UserID = Convert.ToInt32(reader["UserID"]);
                            modelUser.UserName = reader["UserName"].ToString();
                            modelUser.Email = reader["Email"].ToString();
                            modelUser.Password = reader["Password"].ToString();
                            modelUser.MobileNo = reader["MobileNo"].ToString();
                            modelUser.Address = reader["Address"].ToString();
                            modelUser.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]); // Corrected here
                        }
                    }
                }
            }

            return View(modelUser);
        }


        #endregion

        #region Save
        [HttpPost]

        public IActionResult Save(UserModel modelUser)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;

                    
                    if (modelUser.UserID == null || modelUser.UserID == 0)
                    {
                        
                        command.CommandText = "[dbo].[PR_User_Insert]";
                    }
                    else
                    {
                        command.CommandText = "[dbo].[PR_User_Update]";
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelUser.UserID;
                    }

                    command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = modelUser.UserName;
                    command.Parameters.Add("@Email", SqlDbType.VarChar).Value = modelUser.Email;
                    command.Parameters.Add("@Password", SqlDbType.VarChar).Value = modelUser.Password;
                    command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelUser.MobileNo;
                    command.Parameters.Add("@Address", SqlDbType.VarChar).Value = modelUser.Address;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = modelUser.IsActive;

                    if (command.ExecuteNonQuery() > 0)
                    {
                        TempData["UserInsertMsg"] = modelUser.UserID == null ? "Record Inserted Successfully" : "Record Updated Successfully";
                    }
                }
                connection.Close();
            }
            return RedirectToAction("User");
        }


        #endregion


    }
}
