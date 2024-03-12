﻿using AutoMapper;
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
            ShiireJisseki updatedShiireJisseki = _context.ShiireJissekis.FirstOrDefault(s => s.ChumonId == inshiireJisseki.ChumonId && s.ShiireSakiId == inshiireJisseki.ShiireSakiId && s.ShiirePrdId == inshiireJisseki.ShiirePrdId && s.SeqByShiireDate == inshiireJisseki.SeqByShiireDate && s.ShiireDate == inshiireJisseki.ShiireDate);
            if (updatedShiireJisseki != null) {
                // AutoMapperを使用して更新
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<ShiireJisseki, ShiireJisseki>()
                       .EqualityComparison((odto, o) => odto.ChumonId == o.ChumonId && odto.ShiireDate == o.ShiireDate && odto.SeqByShiireDate == o.SeqByShiireDate && odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId);
                });
                var mapper = new Mapper(config);
                mapper.Map(inshiireJisseki, updatedShiireJisseki);

                // 注文残の更新処理
                foreach (ChumonJissekiMeisai meisai in updatedShiireJisseki.ChumonJissekiMeisais.ToList()) {
                    meisai.ChumonZan -= NonyuSu;
                }
            }
            else {
                // 仕入実績が見つからない場合、新しい仕入実績を追加
                _context.ShiireJissekis.Add(inshiireJisseki);
                updatedShiireJisseki = inshiireJisseki;
            }

            // 倉庫在庫の更新
            SokoZaiko updatedSokoZaiko = _context.SokoZaikos.FirstOrDefault(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId);
            if (updatedSokoZaiko != null) {
                // AutoMapperを使用して更新
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<SokoZaiko, SokoZaiko>()
                       .EqualityComparison((odto, o) => odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId && odto.ShohinId == o.ShohinId);
                });
                var mapper = new Mapper(config);
                mapper.Map(insokoZaiko, updatedSokoZaiko);

                // 倉庫在庫数の更新処理
                updatedSokoZaiko.SokoZaikoSu += NonyuSu;
            }
            else {
                // 倉庫在庫が見つからない場合、新しい倉庫在庫を追加
                _context.SokoZaikos.Add(insokoZaiko);
                updatedSokoZaiko = insokoZaiko;
            }

            // 変更を保存
            _context.SaveChanges();

            return (updatedShiireJisseki, updatedSokoZaiko);
        }

    }
}