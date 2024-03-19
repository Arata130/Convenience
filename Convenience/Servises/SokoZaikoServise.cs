using AspNetCore;
using Convenience.Data;
using Convenience.Models.Date.Shiire;
using Convenience.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Convenience.Servises {
    public class SokoZaikoServise : ISokoZaiko {
        private readonly ConvenienceContext _context;
        public SokoZaikoServise(ConvenienceContext context) {
            _context = context;
        }
        public List<SokoZaiko> SokoZaiko { get; set; }

        public List<SokoZaiko> SokoZaikoSearch (bool SortByChumonZan, bool SortBySokoZaikoCaseSu, bool SortBySokoZaikoSu) {
            SokoZaiko = _context.SokoZaikos.Include(s => s.ShiireMaster.ChumonJissekiMeisais)
                .ToList ();

            if (SortByChumonZan == true) {
                // 注文残の小さい順でソート（昇順）
                SokoZaiko = _context.SokoZaikos
                    .Include(s => s.ShiireMaster.ChumonJissekiMeisais)
                    .ToList();
                SokoZaiko.ForEach(s => s.ShiireMaster.ChumonJissekiMeisais = s.ShiireMaster.ChumonJissekiMeisais.OrderBy(c => c.ChumonZan).ToList());
            }
            else {
                // 注文残の多い順でソート（降順）
                SokoZaiko = _context.SokoZaikos
                    .Include(s => s.ShiireMaster.ChumonJissekiMeisais)
                    .ToList();
                SokoZaiko.ForEach(s => s.ShiireMaster.ChumonJissekiMeisais = s.ShiireMaster.ChumonJissekiMeisais.OrderByDescending(c => c.ChumonZan).ToList());
            }

            if (SortBySokoZaikoCaseSu == true) {
                // ケース数順でソート（昇順）
                SokoZaiko = SokoZaiko.OrderBy(s => s.SokoZaikoCaseSu).ToList();
            }
            else {
                // ケース数順でソート（降順）
                SokoZaiko = SokoZaiko.OrderByDescending(s => s.SokoZaikoCaseSu).ToList();
            }

            if (SortBySokoZaikoSu == true) {
                // 在庫数順でソート（昇順）
                SokoZaiko = SokoZaiko.OrderBy(s => s.SokoZaikoSu).ToList();
            }
            else {
                // 在庫数順でソート（降順）
                SokoZaiko = SokoZaiko.OrderByDescending(s => s.SokoZaikoSu).ToList();
            }

            return SokoZaiko;
        }

    }
}
