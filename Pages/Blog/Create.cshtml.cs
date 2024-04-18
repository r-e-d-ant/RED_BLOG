using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RED_BLOG.Pages.Account;
using System.Data.SqlClient;

namespace RED_BLOG.Pages.Blog
{
    [Authorize] // This page will be accessible by only logged in users
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string message = "";
        BlogPost blogPost = new BlogPost();

        public void OnPost()
        {
            try
            {
                blogPost.Title = Request.Form["title"];
                blogPost.Body = Request.Form["body"];
                // blog user from session
                int? blogUserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "blogUserId")?.Value);

                if (blogUserId != null)
                    blogPost.blogUser = blogUserId;

                string connString = _configuration.GetConnectionString("productionDb");

                // saving
                using (SqlConnection con = new SqlConnection(connString))
                {
                    string qry = "INSERT INTO POSTS (title, body, blogUser) VALUES (@v_title, @v_body, @v_user)";

                    // open connex
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(qry, con))
                    {
                        cmd.Parameters.AddWithValue("@v_title", blogPost.Title);
                        cmd.Parameters.AddWithValue("@v_body", blogPost.Body);
                        cmd.Parameters.AddWithValue("@v_user", blogPost.blogUser);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "Blog created";
                            TempData["message"] = "Blog created";
                            Response.Redirect("/Blog/Index");
                        }
                        else
                        {
                            message = "Blog not created";
                            return;
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

    public class BlogPost
    {
        public int? postId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int? blogUser { get; set; }
    }
}
