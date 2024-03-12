using AutoMapper;
using AutoMapper.EquivalencyExpression;
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

        public ShiireJisseki ShiireJisseki { get; set; }
        public SokoZaiko SokoZaiko { get; set; }

        public ShiireJisseki ShiireCreate(ChumonJissekiMeisai inchumonJissekiMeisai) {
            ChumonJissekiMeisai chumonJissekiMeisai = inchumonJissekiMeisai;

            //仕入実績新規作成
            ShiireJisseki shiireJisseki = new ShiireJisseki() {
                ChumonId = chumonJissekiMeisai.ChumonId,
                ShiireDate = DateOnly.FromDateTime(DateTime.Today),
                SeqByShiireDate = 1,
                ShiireDateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                ShiireSakiId = chumonJissekiMeisai.ShiireSakiId,
                ShiirePrdId = chumonJissekiMeisai.ShiirePrdId,
                ShohinId = chumonJissekiMeisai.ShohinId,
                NonyuSu = 0,
                ChumonJissekiMeisais = chumonJissekiMeisai,
            };

            return shiireJisseki;
        }

        public ShiireJisseki ShiireToiawase(string inChumonId, DateOnly inShiireDate) {
            ShiireJisseki = _context.ShiireJissekis
                .Where(s => s.ChumonId == inChumonId)
                .Include(s => s.ChumonJissekiMeisais)
                .ThenInclude(s => s.ChumonJisseki)
                .Include(s => s.ChumonJissekiMeisais)
                .ThenInclude(s => s.ShiireMaster)
                .ThenInclude(s => s.ShiireSakiMaster)
                .Include(s => s.ChumonJissekiMeisais)
                .ThenInclude(s => s.ShiireMaster)
                .ThenInclude(s => s.ShohinMaster)
                .OrderBy(s => s.ShiirePrdId)
                .FirstOrDefault();

            return ShiireJisseki;
        }

        public SokoZaiko ZaikoCreate(ShiireMaster shiireMaster, ShiireJisseki shiireJisseki) {
            SokoZaiko = _context.SokoZaikos.Where(s => s.ShohinId == shiireMaster.ShohinId && s.ShiirePrdId == shiireMaster.ShiirePrdId && s.ShiireSakiId == shiireMaster.ShiireSakiId).FirstOrDefault();

            List<SokoZaiko> sokoZaikos = new List<SokoZaiko>();

            if (ShiireJisseki == null) {
                //商品が一致する在庫がない場合に新規作成
                SokoZaiko = new SokoZaiko() {
                    ShiireSakiId = shiireJisseki.ShiireSakiId,
                    ShiirePrdId = shiireJisseki.ShiirePrdId,
                    ShohinId = shiireJisseki.ShohinId,
                    SokoZaikoCaseSu = 0,
                    SokoZaikoSu = 0,
                    LastShiireDate = DateOnly.MinValue,   //前回がないことを表している
                    LastDeliveryDate = DateOnly.MinValue, //前回がないことを表している
                    ShiireMaster = shiireMaster
                };
            }
            else {
                //直近仕入日を入れて出力
                SokoZaiko = new SokoZaiko() {
                    LastShiireDate = shiireJisseki.ShiireDate,
                    ShiireMaster = shiireMaster
                };
            }

            return SokoZaiko;
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

        public (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko) {
            decimal NonyuSu = inshiireJisseki.NonyuSu;

            // 仕入実績の更新
            ShiireJisseki updatedShiireJisseki = _context.ShiireJissekis
                .Include(s => s.ChumonJissekiMeisais)
                .FirstOrDefault(s => s.ChumonId == inshiireJisseki.ChumonId && s.ShiireSakiId == inshiireJisseki.ShiireSakiId && s.ShiirePrdId == inshiireJisseki.ShiirePrdId && s.SeqByShiireDate == inshiireJisseki.SeqByShiireDate && s.ShiireDate == inshiireJisseki.ShiireDate);
            if (updatedShiireJisseki != null) {
                // 更新
                updatedShiireJisseki.ShiireDateTime = inshiireJisseki.ShiireDateTime; // 仕入日時の更新     

                // 注文残の更新処理
                ChumonJissekiMeisai meisai = updatedShiireJisseki.ChumonJissekiMeisais;
                meisai.ChumonZan -= NonyuSu;

                // 既存のエンティティを取得してプロパティを変更して保存
                var existingMeisai = _context.ChumonJissekiMeisais.FirstOrDefault(c => c.ChumonId == meisai.ChumonId && c.ShiireSakiId == meisai.ShiireSakiId && c.ShiirePrdId == meisai.ShiirePrdId && c.ShohinId == meisai.ShohinId);

                if (existingMeisai != null) {
                    existingMeisai.ChumonZan = meisai.ChumonZan;
                }
            }
            else {
                // 仕入実績が見つからない場合、新しい仕入実績を追加
                ShiireJisseki shiireJisseki = new ShiireJisseki();

                ChumonJissekiMeisai existingMeisai = _context.ChumonJissekiMeisais
                    .FirstOrDefault(c => c.ChumonId == inshiireJisseki.ChumonId && c.ShiireSakiId == inshiireJisseki.ShiireSakiId && c.ShiirePrdId == inshiireJisseki.ShiirePrdId && c.ShohinId == inshiireJisseki.ShohinId);

                if (existingMeisai != null) {
                    // 新しいShiireJissekiをChumonJissekiMeisaiに関連付ける
                    existingMeisai.ShiireJisseki = shiireJisseki;
                }

                updatedShiireJisseki = inshiireJisseki;
                _context.ShiireJissekis.Add(updatedShiireJisseki);
            }

            // 倉庫在庫の更新
            SokoZaiko updatedSokoZaiko = _context.SokoZaikos.FirstOrDefault(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId);
            if (updatedSokoZaiko != null) {
                // 更新
                updatedSokoZaiko.LastShiireDate = insokoZaiko.LastShiireDate; // 直近仕入日の更新
                updatedSokoZaiko.SokoZaikoSu += NonyuSu; // 倉庫在庫数の更新
            }
            else {
                // 倉庫在庫が見つからない場合、新しい倉庫在庫を追加
                updatedSokoZaiko = insokoZaiko;
                _context.SokoZaikos.Add(updatedSokoZaiko);
            }

            // 変更を保存
            _context.SaveChanges();

            return (updatedShiireJisseki, updatedSokoZaiko);
        }

    }
}