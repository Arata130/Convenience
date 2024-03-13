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

        public ShiireJisseki ShiireToiawase(ChumonJissekiMeisai inchumonJissekiMeisai) {
            ShiireJisseki = _context.ShiireJissekis
                .Where(s => s.ChumonId == inchumonJissekiMeisai.ChumonId && s.ShiirePrdId == inchumonJissekiMeisai.ShiirePrdId)
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
            SokoZaiko = _context.SokoZaikos
                .Where(s => s.ShohinId == shiireMaster.ShohinId && s.ShiirePrdId == shiireMaster.ShiirePrdId && s.ShiireSakiId == shiireMaster.ShiireSakiId)
                .FirstOrDefault();

            if (SokoZaiko == null) {
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

        /*public (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko) {
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
                ShiireJisseki shiireJisseki = new ShiireJisseki();

                ChumonJissekiMeisai existingMeisai = _context.ChumonJissekiMeisais
                    .Where(c => c.ChumonId == inshiireJisseki.ChumonId && c.ShiireSakiId == inshiireJisseki.ShiireSakiId && c.ShiirePrdId == inshiireJisseki.ShiirePrdId && c.ShohinId == inshiireJisseki.ShohinId)
                    .FirstOrDefault();

                if (existingMeisai != null) {
                    // 新しいShiireJissekiをChumonJissekiMeisaiに関連付ける
                    existingMeisai.ShiireJissekis = inshiireJisseki;
                }
                _context.SaveChanges();

                ShiireJisseki shiireJisseki1 = _context.ShiireJissekis
                    .Where(c => c.ChumonId == inshiireJisseki.ChumonId && c.ShiireSakiId == inshiireJisseki.ShiireSakiId && c.ShiirePrdId == inshiireJisseki.ShiirePrdId && c.ShohinId == inshiireJisseki.ShohinId)
                    .FirstOrDefault();

                updatedShiireJisseki = inshiireJisseki;
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

                _context.SaveChanges();

                SokoZaiko sokoZaiko = _context.SokoZaikos.Where(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId).FirstOrDefault();
            }

            // 変更を保存
            _context.SaveChanges();

            return (updatedShiireJisseki, updatedSokoZaiko);
        }*/

        public (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko) {

            // 仕入実績の更新
            ShiireJisseki updatedShiireJisseki = _context.ShiireJissekis
                .Include(s => s.ChumonJissekiMeisais)
                .FirstOrDefault(s => s.ChumonId == inshiireJisseki.ChumonId && s.ShiireSakiId == inshiireJisseki.ShiireSakiId && s.ShiirePrdId == inshiireJisseki.ShiirePrdId && s.SeqByShiireDate == inshiireJisseki.SeqByShiireDate && s.ShiireDate == inshiireJisseki.ShiireDate);
            if (updatedShiireJisseki != null) {
                // 既存のエンティティを取得してプロパティを変更して保存
                ChumonJissekiMeisai meisai = updatedShiireJisseki.ChumonJissekiMeisais;
                ChumonJissekiMeisai existingMeisai = _context.ChumonJissekiMeisais.FirstOrDefault(c => c.ChumonId == meisai.ChumonId && c.ShiireSakiId == meisai.ShiireSakiId && c.ShiirePrdId == meisai.ShiirePrdId && c.ShohinId == meisai.ShohinId);

                if (existingMeisai != null) {
                    existingMeisai.ChumonZan -= inshiireJisseki.NonyuSu;  //注文実績明細　注文残の計算
                    existingMeisai.ShiireJissekis.ShiireDateTime = DateTime.UtcNow;  //仕入実績　仕入日時を現時刻に修正
                }

                //DBに保存
                _context.SaveChanges();
            }
            else {  
                /*
                 * 新規作成
                 * 注文残　→　注文残 - 納入数
                 */
                ChumonJissekiMeisai existingMeisai = _context.ChumonJissekiMeisais
                    .Where(c => c.ChumonId == inshiireJisseki.ChumonId && c.ShiireSakiId == inshiireJisseki.ShiireSakiId && c.ShiirePrdId == inshiireJisseki.ShiirePrdId && c.ShohinId == inshiireJisseki.ShohinId)
                    .FirstOrDefault();

                if (existingMeisai != null) {
                    // 新しいShiireJissekiをChumonJissekiMeisaiに関連付ける
                    existingMeisai.ShiireJissekis = inshiireJisseki;
                    if (existingMeisai.ChumonZan >= existingMeisai.ShiireJissekis.NonyuSu) {
                        //注文実績明細の注文数が仕入実績の納入数以上の場合、
                        //注文残から納入数の分を引く
                        existingMeisai.ChumonZan -= existingMeisai.ShiireJissekis.NonyuSu;
                    }
                }

                //DBに保存
                _context.SaveChanges();

                //DB保存確認用　完成時削除
                ShiireJisseki shiireJisseki1 = _context.ShiireJissekis
                    .Where(c => c.ChumonId == inshiireJisseki.ChumonId && c.ShiireSakiId == inshiireJisseki.ShiireSakiId && c.ShiirePrdId == inshiireJisseki.ShiirePrdId && c.ShohinId == inshiireJisseki.ShohinId)
                    .FirstOrDefault();

                //返り値をセットする
                updatedShiireJisseki = existingMeisai.ShiireJissekis;
            }

            // 倉庫在庫の更新
            SokoZaiko updatedSokoZaiko = _context.SokoZaikos
                .Include(s => s.ShiireMaster)
                .FirstOrDefault(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId);
            if (updatedSokoZaiko != null) {
                // 更新
                updatedSokoZaiko.LastShiireDate = inshiireJisseki.ShiireDate; // 直近仕入日の更新
                updatedSokoZaiko.SokoZaikoSu += inshiireJisseki.NonyuSu; // 倉庫在庫数の更新

                _context.SaveChanges();
            }
            else {
                ShiireMaster existingShiireMaster = _context.ShiireMasters
                    .Where(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId)
                    .FirstOrDefault();

                if (existingShiireMaster != null) {
                    // 新しいSokoZaikoをShiireMasterに関連付ける
                    existingShiireMaster.SokoZaikos = insokoZaiko;
                    existingShiireMaster.SokoZaikos.SokoZaikoSu = inshiireJisseki.NonyuSu;
                }

                //DBに保存
                _context.SaveChanges();

                //DB保存確認用　完成時削除
                SokoZaiko sokoZaiko = _context.SokoZaikos.Where(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId).FirstOrDefault();
            }

            return (updatedShiireJisseki, updatedSokoZaiko);
        }
    }
}