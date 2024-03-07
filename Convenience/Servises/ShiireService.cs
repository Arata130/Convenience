using Convenience.Data;
using Convenience.Models.Date.Chumon;
using Convenience.Models.Date.Shiire;
using Convenience.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Convenience.Servises {
    public class ShiireService : IShiire {
        private readonly ConvenienceContext _context;

        public ShiireService(ConvenienceContext context) {
            _context = context;
        }

        public List<ShiireJisseki> ShiireJissekis { get; set; }
        public List<SokoZaiko> SokoZaikos { get; set; }

        public List<ShiireJisseki> ShiireCreate(string inChumonId) {
            //注文実績を注文コードにて検索
            List<ChumonJissekiMeisai> chumonJissekiMeisais = _context.ChumonJissekiMeisais
                .Where(c => c.ChumonId == inChumonId)
                .Include(c => c.ChumonJisseki)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShiireSakiMaster)
                .OrderBy(c => c.ShiirePrdId)
                .ToList();

            List<ShiireJisseki> shiireJissekis = new List<ShiireJisseki>(); 

            //仕入実績新規作成
            foreach (var chumonJissekiMeisai in chumonJissekiMeisais) {
                ShiireJisseki shiireJisseki = new ShiireJisseki() {
                    ChumonId = inChumonId,
                    ShiireDate = DateOnly.FromDateTime(DateTime.Today),
                    SeqByShiireDate = 1,
                    ShiireDateTime = DateTime.Now,
                    ShiireSakiId = chumonJissekiMeisai.ShiireSakiId,
                    ShiirePrdId = chumonJissekiMeisai.ShiirePrdId,
                    ShohinId = chumonJissekiMeisai.ShohinId,
                    NonyuSu = 0,
                    ChumonJissekiMeisais = chumonJissekiMeisai,
                };
                shiireJissekis.Add(shiireJisseki);
            }

            ShiireJissekis = shiireJissekis;

            return shiireJissekis;
        }

        public ShiireJisseki ShiireToiawase(string inChumonId) {
            ShiireJisseki shiireJisseki = _context.ShiireJissekis
                .Where(s => s.ChumonId == inChumonId)
                .Include(s => s.ChumonJissekiMeisais)
                .ThenInclude(s => s.ShiireMaster)
                .ThenInclude(s => s.ShohinMaster)
                .OrderBy(s => s.ShiirePrdId)
                .FirstOrDefault();

            return shiireJisseki;
        }

        public List<SokoZaiko> ZaikoCreate (string inChumonId) {
            ShiireService shiire = new ShiireService(_context);

            List<ShiireJisseki> shiireJissekis = shiire.ShiireCreate (inChumonId);

            List<SokoZaiko> sokoZaikos = new List<SokoZaiko> ();

            foreach (var shiireJisseki in shiireJissekis) {
                SokoZaiko sokoZaiko = new SokoZaiko() {
                    ShiireSakiId = shiireJisseki.ShiireSakiId,
                    ShiirePrdId = shiireJisseki.ShiirePrdId,
                    ShohinId = shiireJisseki.ShohinId,
                    SokoZaikoCaseSu = 0,
                    SokoZaikoSu = 0,
                    LastShiireDate = DateOnly.MinValue,   //前回がないことを表している
                    LastDeliveryDate = DateOnly.MinValue,　//前回がないことを表している
                };
                sokoZaikos.Add(sokoZaiko);
            }
            SokoZaikos = sokoZaikos;

            return SokoZaikos;
        }

        public ShiireJisseki ChumonZanBalance(ShiireJisseki inshiireJisseki) {
            decimal NonyuSu = inshiireJisseki.NonyuSu;
            decimal ChumonZan = inshiireJisseki.ChumonJissekiMeisais.ChumonZan;

            if (ChumonZan >= NonyuSu) {
                inshiireJisseki.ChumonJissekiMeisais.ChumonZan -= NonyuSu;
            }
            else {
                //エラーイベント：納入数が注文数より多いため、数量の間違いあり
            }
            ShiireJisseki shiireJisseki = inshiireJisseki;

            return shiireJisseki;
        }

        public (ShiireJisseki shiireJisseki, SokoZaiko sokoZaiko) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko) {
            ShiireJisseki shiireJisseki = inshiireJisseki;
            SokoZaiko sokoZaiko = insokoZaiko;

            shiireJisseki = ChumonZanBalance(shiireJisseki);
            sokoZaiko.SokoZaikoSu += shiireJisseki.NonyuSu;


            return (shiireJisseki, sokoZaiko);
        }
    }
}
