using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Convenience.Models.Date.Chumon;
using Convenience.Models.Date.Shiire;

namespace Convenience.Data
{
    public class ConvenienceContext : DbContext {
        public ConvenienceContext(DbContextOptions<ConvenienceContext> options) : base(options) {
        }

        public virtual DbSet<ShohinMaster> ShohinMasters { get; set; }
        public virtual DbSet<ShiireSakiMaster> ShiireSakiMasters { get; set; }
        public virtual DbSet<ShiireMaster> ShiireMasters { get; set; }
        public virtual DbSet<ChumonJisseki> ChumonJissekis { get; set; }
        public virtual DbSet<ChumonJissekiMeisai> ChumonJissekiMeisais { get; set; }
        public virtual DbSet<ShiireJisseki> ShiireJissekis { get; set; }
        public virtual DbSet<SokoZaiko> SokoZaikos { get; set; }
    }
}
