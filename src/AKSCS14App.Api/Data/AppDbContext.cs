using Microsoft.EntityFrameworkCore;

namespace CS14App.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}