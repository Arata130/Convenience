using Convenience.Models.Date.Chumon;
using Convenience.Models.Date.Shiire;

namespace Convenience.Models.Interfaces {

    public interface IShiire {
        public ShiireJisseki ShiireJisseki { get; set; }
        public SokoZaiko SokoZaiko { get; set; }
        public ShiireJisseki ShiireToiawase(ChumonJissekiMeisai chumonJissekiMeisai);
        public ShiireJisseki ShiireCreate(ChumonJissekiMeisai chumonJissekiMeisai);
        public SokoZaiko ZaikoCreate(ShiireMaster shiireMaster, ShiireJisseki shiireJisseki);
        public ShiireJisseki ChumonZanBalance(ShiireJisseki inshiireJisseki);
        public (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko insokoZaiko);
    }
}