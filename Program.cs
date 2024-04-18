var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddAuthentication("RedBlogCookieAuth").AddCookie("RedBlogCookieAuth", options =>
{
    options.Cookie.Name = "RedBlogCookieAuth";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    // options.ExpireTimeSpan = TimeSpan.FromMinutes(20); expiration time
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.RequireClaim("role", "ADMIN")); // policy name
    options.AddPolicy("BloggerOnly",
        policy => policy.RequireClaim("role", "BLOGGER")); // policy name
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapRazorPages();

app.Run();
