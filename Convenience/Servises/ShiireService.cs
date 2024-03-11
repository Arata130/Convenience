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
        public List<ShiireJisseki> ShiireJissekis { get; set; }
        public List<SokoZaiko> SokoZaikos { get; set; }

        public ShiireJisseki ShiireCreate(ChumonJissekiMeisai inchumonJissekiMeisai) {
            ChumonJissekiMeisai chumonJissekiMeisai = inchumonJissekiMeisai;

            //仕入実績新規作成
            ShiireJisseki shiireJisseki = new ShiireJisseki() {
                ChumonId = chumonJissekiMeisai.ChumonId,
                ShiireDate = DateOnly.FromDateTime(DateTime.Today),
                SeqByShiireDate = 1,
                ShiireDateTime = DateTime.UtcNow,
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
                    ShiireDateTime = DateTime.UtcNow,
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

        public List<SokoZaiko> ZaikoCreate(string inChumonId) {
            ShiireService shiire = new ShiireService(_context);
            List<ShiireJisseki> shiireJissekis = shiire.ShiireCreate(inChumonId);

            List<SokoZaiko> sokoZaikos = new List<SokoZaiko>();

            foreach (var shiireJisseki in shiireJissekis) {
                SokoZaiko sokoZaiko = new SokoZaiko() {
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

        public SokoZaiko SokoZaikoSuCal(ShiireJisseki inshiireJisseki) {
            SokoZaiko sokoZaiko = null;

            sokoZaiko.SokoZaikoSu += inshiireJisseki.NonyuSu;
            return sokoZaiko;
        }

        public (ShiireJisseki shiireJisseki, SokoZaiko sokoZaiko) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko) {
            decimal NonyuSu = inshiireJisseki.NonyuSu;

            ShiireJisseki shiireJisseki = _context.ShiireJissekis.SingleOrDefault(s => s.ChumonId == inshiireJisseki.ChumonId && s.ShiireSakiId == inshiireJisseki.ShiireSakiId && s.ShiirePrdId == inshiireJisseki.ShiirePrdId && s.SeqByShiireDate == ShiireJisseki.SeqByShiireDate && s.ShiireDate == inshiireJisseki.ShiireDate);

            SokoZaiko sokoZaiko = _context.SokoZaikos.SingleOrDefault(s => s.ShiireSakiId == insokoZaiko.ShiireSakiId && s.ShiirePrdId == insokoZaiko.ShiirePrdId && s.ShohinId == insokoZaiko.ShohinId);

            if (shiireJisseki != null) {
                // ShiireJissekiの更新
                ShiireJisseki existedshiireJisseki = _context.ShiireJissekis
                    .Include(e => e.ChumonJissekiMeisais)
                    .FirstOrDefault(e => e.ChumonId == inshiireJisseki.ChumonId && e.ShiireDate == inshiireJisseki.ShiireDate && e.SeqByShiireDate == inshiireJisseki.SeqByShiireDate && e.ShiireSakiId == inshiireJisseki.ShiireSakiId && e.ShiirePrdId == inshiireJisseki.ShiirePrdId);

                // AutoMapperを使用して更新
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<ShiireJisseki, ShiireJisseki>()
                       .EqualityComparison((odto, o) => odto.ChumonId == o.ChumonId && odto.ShiireDate == o.ShiireDate && odto.SeqByShiireDate == o.SeqByShiireDate && odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId);
                    cfg.CreateMap<ChumonJissekiMeisai, ChumonJissekiMeisai>()
                       .ForMember(dest => dest.ChumonZan, opt => opt.MapFrom(src => src.ChumonZan - NonyuSu));
                });
                var mapper = new Mapper(config);
                mapper.Map(inshiireJisseki, existedshiireJisseki);

                shiireJisseki = existedshiireJisseki;
            }
            else {
                foreach (var item in ShiireJissekis) {
                    item.ShiireDateTime = DateTime.UtcNow;
                }
                _context.ShiireJissekis.Add(inshiireJisseki);
            }

            if (sokoZaiko != null) {
                // SokoZaikoの更新
                SokoZaiko existedsokoZaiko = _context.SokoZaikos.FirstOrDefault(e => e.ShiireSakiId == sokoZaiko.ShiireSakiId && e.ShiirePrdId == sokoZaiko.ShiirePrdId && e.ShohinId == sokoZaiko.ShohinId);

                // AutoMapperを使用して更新
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<SokoZaiko, SokoZaiko>()
                       .EqualityComparison((odto, o) => odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId && odto.ShohinId == o.ShohinId);
                });
                var mapper = new Mapper(config);
                mapper.Map(insokoZaiko, existedsokoZaiko);

                // SokoZaikoSuをNonyuSuを足す処理
                existedsokoZaiko.SokoZaikoSu += NonyuSu;

                sokoZaiko = existedsokoZaiko;
            }
            else {
                foreach (var item in SokoZaikos) {
                    item.SokoZaikoSu += shiireJisseki.NonyuSu;
                }
                _context.ShiireJissekis.Add(shiireJisseki);

            sokoZaiko.SokoZaikoSu += inshiireJisseki.NonyuSu;

            //注文実績の検索（キー注文コード）
            var isshiireJisseki = _context.ChumonJissekis.Find(shiireJisseki.ChumonId, shiireJisseki.ShiireSakiId);

            if (isshiireJisseki != null) {  //注文実績がある場合
                                            //注文実績を読む
                var existedshiireJisseki = _context.ShiireJissekis
                    .Include(e => e.ChumonJissekiMeisais)
                    .FirstOrDefault(e => e.ChumonId == shiireJisseki.ChumonId);

                //引数で渡された注文実績データを現プロパティに反映する
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<ShiireJisseki, ShiireJisseki>()
                    .EqualityComparison((odto, o) => odto.ChumonId == o.ChumonId && odto.ShiireDate == o.ShiireDate && odto.SeqByShiireDate == o.SeqByShiireDate && odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId);
                    cfg.CreateMap<SokoZaiko, SokoZaiko>()
                    .EqualityComparison((odto, o) => odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId && odto.ShohinId == o.ShohinId);
                });

                //引数で渡された注文実績をDBから読み込んだ注文実績に上書きする
                var mapper = new Mapper(config);
                mapper.Map(shiireJisseki, existedshiireJisseki);

                shiireJisseki = existedshiireJisseki;
            }

            return (shiireJisseki, sokoZaiko);

        }
    }
}
