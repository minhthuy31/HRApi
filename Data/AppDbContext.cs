using HRApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<ChucVuNhanVien> ChucVuNhanViens { get; set; }
        public DbSet<HopDong> HopDongs { get; set; }
        public DbSet<ChuyenNganh> ChuyenNganhs { get; set; }
        public DbSet<TrinhDoHocVan> TrinhDoHocVans { get; set; }
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<DonNghiPhep> DonNghiPheps { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }
        public DbSet<ActiveQRToken> ActiveQRTokens { get; set; }
        public DbSet<DangKyOT> DangKyOTs { get; set; }
        public DbSet<DangKyCongTac> DangKyCongTacs { get; set; }
        public DbSet<KhoaCong> KhoaCongs { get; set; }
    }
}
