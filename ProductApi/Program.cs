using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;

// 1. ตั้งค่า Database (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

// 2. เพิ่ม Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. ตั้งค่า CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins(Configuration["Jwt:Audience"]!) // อ่านจาก appsettings
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 4. ตั้งค่า Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. เปิดใช้งาน Authentication
app.UseAuthentication();
app.UseAuthorization();

// --- API Endpoints ---

// POST: /api/auth/login
app.MapPost("/api/auth/login", async (LoginModel login, AppDbContext db) =>
{
    var user = await db.Stores.FirstOrDefaultAsync(u => u.Email == login.Email);

    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        // new Claim(ClaimTypes.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: Configuration["Jwt:Issuer"],
        audience: Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: credentials);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString, email = user.Email });
});

// POST: /api/auth/register
app.MapPost("/api/auth/register", async (RegisterModel registerModel, AppDbContext db) =>
{
    if (!await db.Stores.AnyAsync(u => u.Email == registerModel.Email))
    {

        var userModel = new Store
        {
            Email = registerModel.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerModel.Password),
            StoreName = registerModel.StoreName,
        };

        db.Stores.Add(userModel);
        await db.SaveChangesAsync();

        return Results.Ok("สร้าง store สำเร็จ");
    }

    return Results.BadRequest("มี Store นี้ในระบบแล้ว");
});

// POST: /api/auth/logout
app.MapPost("/api/auth/logout", () =>
{
    return Results.Ok("Logged out successfully.");
}).RequireAuthorization();

// GET: /api/store/information
app.MapGet("/api/store/information", async (AppDbContext db, ClaimsPrincipal storeUser) =>
{
    try
    {
        // 1. ดึง User ID จาก Token (Claim "Sub")
        if (!int.TryParse(storeUser.FindFirstValue(ClaimTypes.NameIdentifier), out var currentStoreId))
        {
            return Results.Unauthorized();
        }

        var storeInfo = await db.Stores.Where(store => store.Id == currentStoreId).Select(e => new { e.Id, e.Email, e.StoreName }).Distinct().FirstOrDefaultAsync();
        return Results.Ok(storeInfo);
    }
    catch (Exception e)
    {
        return Results.Problem("something went wrong.");
    }
})
.RequireAuthorization();

// --- Product Endpoints (ต้อง Login ทั้งหมด) ---

// GET: /api/products
app.MapGet("/api/products", async (AppDbContext db, ClaimsPrincipal user) =>
{

    // 1. ดึง User ID จาก Token (Claim "Sub")
    if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
    {
        return Results.Unauthorized();
    }

    // 2. ‼️ กรองข้อมูลด้วย Where() ‼️
    var products = await db.Products
        .Where(p => p.StoreId == currentUserId) // <-- ‼️ สำคัญมาก ‼️
        .OrderBy(p => p.Id)
        .ToListAsync();

    return Results.Ok(products);


})
.RequireAuthorization();

// POST: /api/products
app.MapPost("/api/products", async (Product productReq, AppDbContext db, ClaimsPrincipal storeUser) =>
{
    // 1. ดึง User ID จาก Token
    if (!int.TryParse(storeUser.FindFirstValue(ClaimTypes.NameIdentifier), out var currentStoreId))
    {
        return Results.Unauthorized();
    }

    // 2. ‼️ ตั้งค่า Owner ของ Product ‼️
    productReq.StoreId = currentStoreId; // <-- ‼️ สำคัญมาก ‼️

    // 3. (Validation เหมือนเดิม)
    if (productReq.ProductCode.Length != 16)
        return Results.BadRequest("รหัสสินค้าต้องมี 16 หลัก");
    if (!Regex.IsMatch(productReq.ProductCode, "^[A-Z0-9]{16}$"))
        return Results.BadRequest("รหัสสินค้าต้องเป็นตัวเลข (0-9) หรือตัวอักษรภาษาอังกฤษพิมพ์ใหญ่ (A-Z) เท่านั้น");

    // ‼️ การเช็คซ้ำ: เราจะเช็คซ้ำ "ทั้งระบบ" (ไม่ใช่แค่ของ User)
    // เพราะ Product Code (SKU) ควรจะเป็น Unique 
    if (await db.Products.AnyAsync(p => (p.ProductCode == productReq.ProductCode && p.StoreId == currentStoreId)))
        return Results.BadRequest("รหัสสินค้านี้มีในระบบแล้ว");

    // 4. (Save เหมือนเดิม)
    db.Products.Add(productReq);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{productReq.Id}", productReq);
})
.RequireAuthorization();

// DELETE: /api/products/{id}
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db, ClaimsPrincipal storeUser) =>
{
    // 1. ดึง User ID จาก Token
    if (!int.TryParse(storeUser.FindFirstValue(ClaimTypes.NameIdentifier), out var currentStoreId))
    {
        return Results.Unauthorized();
    }

    // 2. หา Product
    var product = await db.Products.FindAsync(id);
    if (product == null) return Results.NotFound();

    // 3. ‼️ ตรวจสอบความเป็นเจ้าของ ‼️
    if (product.StoreId != currentStoreId)
    {
        // ถ้าไม่ใช่เจ้าของ: ห้ามลบ!
        return Results.Forbid(); // หรือ Results.NotFound() เพื่อซ่อนว่ามี ID นี้
    }

    // 4. (ลบ เหมือนเดิม)
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization();

// --- 6. สร้างฐานข้อมูลและ User Test ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // สั่งสร้างตารางใน DB (ถ้ายังไม่มี)
    dbContext.Database.EnsureCreated();

    // เพิ่ม User test ถ้ายังไม่มี
    if (!await dbContext.Stores.AnyAsync(u => u.Email == "admin@test.com"))
    {
        var testUser = new Store
        {
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), // รหัสคือ Password123!
            StoreName = "my first store name"
        };
        dbContext.Stores.Add(testUser);
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
