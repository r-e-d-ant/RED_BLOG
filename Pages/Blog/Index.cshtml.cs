using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RED_BLOG.Pages.Account;
using System.Data.SqlClient;

namespace RED_BLOG.Pages.Blog
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public string? message = null;

        public List<MyBlogs> blogsList = new List<MyBlogs>();
        public BlogUser blogUser = new BlogUser();

        public void OnGet()
        {
            if (TempData.Count > 0)
                message = TempData["Message"] as string;

            blogsList.Clear(); // clear blogs list

            // display blogs
            loadBlogs();

            // display logged in user information
            string? bloggerEmail = User.Claims.FirstOrDefault(c => c.Type == "blogUserEmail")?.Value;

            if (bloggerEmail != null)
                loadLoggedInUserInformation(bloggerEmail);
        }

        // load loggedIn user information
        public void loadLoggedInUserInformation(string bloggerEmail)
        {
            try
            {
                string connString = _configuration.GetConnectionString("productionDb");
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string getBlogsQuery = "SELECT userId, fullname, email, role FROM blog_users WHERE email=@v_email";

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(getBlogsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("v_email", bloggerEmail);

                        // read user info
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                blogUser.userId = Int32.Parse(reader["userId"].ToString());
                                blogUser.fullname = reader["fullname"].ToString();
                                blogUser.email = reader["email"].ToString();
                                blogUser.role = reader["role"].ToString();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }
        }

        // load blogs
        public void loadBlogs()
        {
            try
            {
                string connString = _configuration.GetConnectionString("productionDb");
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string getBlogsQuery = "SELECT po.postId, po.title, po.body, po.blogUser, bu.userId as bloggerId, bu.fullname as blogger, createdAt FROM POSTS po JOIN blog_users bu ON bu.userId = po.blogUser ORDER BY createdAt DESC";

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(getBlogsQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // read blogs
                            while (reader.Read())
                            {
                                MyBlogs blog = new MyBlogs();

                                blog.postId = Int32.Parse(reader["postId"].ToString());
                                blog.Title = reader["Title"].ToString();
                                blog.Body = reader["body"].ToString();
                                blog.bloggerId = Int32.Parse(reader["bloggerId"].ToString());
                                blog.blogger = reader["blogger"].ToString();
                                blog.postedTime = DateTime.Parse(reader["createdAt"].ToString());

                                blogsList.Add(blog);
                            }
                        }
                    }
                    conn.Close();
                }
            } catch(Exception ex)
            {
                message = "Error: " + ex.Message;
            }
        }
    }

    public class MyBlogs
    {
        public int? postId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int? bloggerId { get; set; }
        public string? blogger { get; set; }
        public DateTime? postedTime { get; set; }
    }
}
