using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Convenience.Models.Interfaces;
using Convenience.Data;
using Convenience.Models.Date.Chumon;

namespace Convenience.Servises
{
    public class ChumonService : IChumon {
        private readonly ConvenienceContext _context;
        public ChumonService(ConvenienceContext context) {
            _context = context;
        }

        public ChumonJisseki chumonJisseki { get; set; }
        //public ChumonJisseki ChumonJisseki { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ChumonJisseki ChumonCreate(string inShiireSakiId) {
            //注文実績に新登録分
            ChumonJisseki chumonJisseki = new ChumonJisseki() {
                ChumonId = ChumonIdCreate(),
                ShiireSakiId = inShiireSakiId,
                ChumonDate = DateOnly.FromDateTime(DateTime.Today)
            };

            List<ShiireMaster> shiireMasters = _context.ShiireMasters
                .Where(c => c.ShiireSakiId == inShiireSakiId)
                .Include(c => c.ShohinMaster)
                .OrderBy(c => c.ShiirePrdId)
                .ToList();

            List<ChumonJissekiMeisai> chumonJissekiMeisais = new List<ChumonJissekiMeisai>();

            foreach (ShiireMaster shiire in shiireMasters) {
                ChumonJissekiMeisai chumonJissekiMeisai = new ChumonJissekiMeisai() {
                    ChumonId = chumonJisseki.ChumonId,
                    ShiireSakiId = inShiireSakiId,
                    ShiirePrdId = shiire.ShiirePrdId,
                    ShohinId = shiire.ShohinId,
                    ChumonSu = 0,
                    ChumonZan = 0,
                    ShiireMaster = shiire
                };
                chumonJissekiMeisais.Add(chumonJissekiMeisai);
            }
            chumonJisseki.ChumonJissekiMeisais = chumonJissekiMeisais;

            return chumonJisseki;
        }

     /*
      *  注文コード発番
      *  引数　  なし
      *  戻り値　注文コード
      *  
      *  注文コード書式例）：20240129-001(yyyyMMdd-000～999）
      */
        public string ChumonIdCreate() {
            int seqnumber;
            string returnchumonid;

            //今日の日付
            string datearea = DateTime.Today.ToString("yyyyMMdd");

            //今日の日付からすでに今日の分の注文コードがないか調べる
            var chumonid = _context.ChumonJissekis
                .Where(x => x.ChumonId.StartsWith(datearea))
                .Max(x => x.ChumonId);

            if (string.IsNullOrEmpty(chumonid)) //今日、注文コード起こすのが初めての場合
            {
                seqnumber = 1;
            }
            else {                              //上記以外の場合
                seqnumber = Int32.Parse(chumonid.Substring(9, 3));  //注文コードの右３桁の数値を求める
                seqnumber++;                                        //上記求めた後に＋１をする
            }

            if (seqnumber <= 999) {  //３桁の数値が999以内（ＯＫ）
                returnchumonid = datearea + "-" + seqnumber.ToString("000");
            }
            else {                  //999以上はＮＵＬＬセット
                returnchumonid = null;
            }

            //発番後の注文コードを戻り値にセット
            return (returnchumonid);
        }


        public ChumonJisseki ChumonToiawase(string inShireSakiId, DateOnly inChumonDate) {
            ChumonJisseki chumonJisseki = _context.ChumonJissekis
                .Where(c => c.ShiireSakiId == inShireSakiId && c.ChumonDate == inChumonDate)
                .Include(c => c.ChumonJissekiMeisais)
                .ThenInclude(c => c.ShiireMaster)
                .ThenInclude(c => c.ShohinMaster)
                .FirstOrDefault();
            return chumonJisseki;
        }

        public ChumonJisseki ChumonUpdate(ChumonJisseki inChumonJisseki) {
            /*
             * 注文実績＋注文明細更新
             *  引数　  注文実績
             *  戻り値　注文実績
             */
            //プロパティ注文実績に引数の注文実績をセットする
            chumonJisseki = inChumonJisseki;

            //注文実績の検索（キー注文コード）
            var isChumonJisseki = _context.ChumonJissekis.Find(chumonJisseki.ChumonId, chumonJisseki.ShiireSakiId);

            if (isChumonJisseki != null) {  //注文実績がある場合
                                            //注文実績を読む
                var existedChumonJisseki = _context.ChumonJissekis
                    .Include(e => e.ChumonJissekiMeisais)
                    .FirstOrDefault(e => e.ChumonId == chumonJisseki.ChumonId);

                //引数で渡された注文実績データを現プロパティに反映する
                var config = new MapperConfiguration(cfg => {
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<ChumonJisseki, ChumonJisseki>()
                    .EqualityComparison((odto, o) => odto.ChumonId == o.ChumonId);
                    cfg.CreateMap<ChumonJissekiMeisai, ChumonJissekiMeisai>()
                    .EqualityComparison((odto, o) => odto.ChumonId == o.ChumonId && odto.ShiireSakiId == o.ShiireSakiId && odto.ShiirePrdId == o.ShiirePrdId && odto.ShohinId == o.ShohinId)
                    .BeforeMap((src, dest) => src.LastChumonSu = dest.ChumonSu)
                    .ForMember(dest => dest.ChumonZan, opt => opt.MapFrom(src => src.ChumonZan + src.ChumonSu - src.LastChumonSu))
                    .ForMember(dest => dest.ChumonJisseki, opt => opt.Ignore());
                });

                //引数で渡された注文実績をDBから読み込んだ注文実績に上書きする
                var mapper = new Mapper(config);
                mapper.Map(chumonJisseki, existedChumonJisseki);

                chumonJisseki = existedChumonJisseki;

            }
            else {   //注文実績がない場合、引数で渡された注文実績をDBにレコード追加する
                foreach (var item in chumonJisseki.ChumonJissekiMeisais) {
                    item.ChumonZan = item.ChumonSu;
                }
                _context.ChumonJissekis.Add(chumonJisseki);
            }
            //注文実績＋注文実績明細を戻り値とする
            return chumonJisseki;
        }
    }
}
