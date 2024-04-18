using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;

namespace RED_BLOG.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string message = "";

        public Credential credential = new Credential();

        public void OnGet()
        {
            //
        }

        public async Task<IActionResult> OnPostAsync()
        {
            credential.email = Request.Form["email"];
            credential.password = Request.Form["password"];

            if (VerifyCredentialFromDatabase(credential.email, credential.password))
            {
                // Get the user information from the database
                MyBlogUsers blogUser = GetUserFromDatabase(credential.email);

                if (blogUser != null)
                {
                    // Creating the security context
                    var claims = new List<Claim>
                    {
                        new Claim("blogUserId", blogUser.userId + ""),
                        new Claim("blogUserEmail", blogUser.email),
                        new Claim("blogUserRole", blogUser.role)
                    };

                    // Add claims to identity, also specify authentication name (anyname)
                    var identity = new ClaimsIdentity(claims, "RedBlogCookieAuth");
                    // Principal contains the security context, and can have many identities
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                    // Serialize claims principal into a string
                    // Then encrypt that string, save that as a cookie in the HttpContext
                    await HttpContext.SignInAsync("RedBlogCookieAuth", claimsPrincipal);

                    return RedirectToPage("/Blog/Index");
                }
                else
                {
                    return Page();
                }

            }
            message = "Wrong credentials, try again";
            return Page();
        }

        private bool VerifyCredentialFromDatabase(string userEmail, string password)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("productionDb");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Open the database connection
                    conn.Open();

                    // Prepare the SQL query
                    string qry = "SELECT userId, password FROM blog_users WHERE email=@v_email";

                    // Create the command and parameters
                    using (SqlCommand command = new SqlCommand(qry, conn))
                    {
                        command.Parameters.AddWithValue("@v_email", userEmail);

                        // Execute the query and retrieve the result
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if the user exists and the password is valid
                            if (reader.Read())
                            {
                                string storedHashedPassword = reader.GetString("password");

                                return BCrypt.Net.BCrypt.Verify(password, storedHashedPassword);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }

            return false;
        }

        private MyBlogUsers GetUserFromDatabase(string email)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("productionDb");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Open the database connection
                    conn.Open();

                    // Prepare the SQL query
                    string qry = "SELECT userId, fullname, email, [password], [role] FROM blog_users WHERE email=@v_email";

                    // Create the command and parameters
                    using (SqlCommand command = new SqlCommand(qry, conn))
                    {
                        command.Parameters.AddWithValue("@v_email", email);

                        // Execute the query and retrieve the result
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Retrieve the user information from the reader
                                int bloggerUserId = reader.GetInt32("userId");
                                string bloggerFullname = reader.GetString("fullname");
                                string bloggerEmail = reader.GetString("email");
                                string bloggerPassword = reader.GetString("password");
                                string bloggerRole = reader.GetString("role");

                                // Create a User object with the retrieved information
                                MyBlogUsers blogUser = new MyBlogUsers
                                {
                                    userId = bloggerUserId,
                                    fullname = bloggerFullname,
                                    email = bloggerEmail,
                                    password = bloggerPassword,
                                    role = bloggerRole
                                };

                                return blogUser;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }


            return null;
        }
    }

    public class Credential
    {
        public string? email { get; set; }
        public string? password { get; set; }
    }

    public class MyBlogUsers
    {
        public int? userId { get; set; }
        public string? fullname { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
    }
}
