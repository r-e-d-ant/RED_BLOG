using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace RED_BLOG.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public RegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        BlogUser blogUser = new BlogUser();

        public string message = "";
        public void OnPost()
        {
            try
            {
                blogUser.fullname = Request.Form["fullname"];
                blogUser.email = Request.Form["email"];
                blogUser.password = Request.Form["password"];

                string connString = _configuration.GetConnectionString("productionDb");
                // saving
                using (SqlConnection con = new SqlConnection(connString))
                {
                    string qry = "INSERT INTO blog_users (fullname, email, password) VALUES (@v_fullname, @v_email, @v_password)";

                    // open connex
                    con.Open();

                    using(SqlCommand cmd = new SqlCommand(qry, con))
                    {
                        cmd.Parameters.AddWithValue("@v_fullname", blogUser.fullname);
                        cmd.Parameters.AddWithValue("@v_email", blogUser.email);

                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(blogUser.password); // hash password using encrypt
                        cmd.Parameters.AddWithValue("@v_password", hashedPassword);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "User created, You can login now";
                        } else
                        {
                            message = "User not created";
                        }
                    }

                    // close connection
                    con.Close();
                }
            } catch (Exception ex)
            {
                message = "Error: " + ex.ToString();
            }
        }
    }

    public class BlogUser
    {
        public int? userId { get; set; }
        public string? fullname { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
    }
}
