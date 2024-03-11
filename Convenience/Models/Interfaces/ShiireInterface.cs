using Convenience.Models.Date.Chumon;
using Convenience.Models.Date.Shiire;

namespace Convenience.Models.Interfaces {

    public interface IShiire {
        public ShiireJisseki ShiireJisseki { get; set; }
        public SokoZaiko SokoZaiko { get; set; }
        public List<ShiireJisseki> ShiireJissekis { get; set; }
        public List<SokoZaiko> SokoZaikos { get; set; }
        public ShiireJisseki ShiireToiawase(string inChumonId, DateOnly ShiireDate);
        public ShiireJisseki ShiireCreate(ChumonJissekiMeisai chumonJissekiMeisai);
        public SokoZaiko ZaikoCreate(ShiireMaster shiireMaster, ShiireJisseki shiireJisseki);
        public ShiireJisseki ChumonZanBalance(ShiireJisseki inshiireJisseki);
        public (ShiireJisseki shiireJisseki, SokoZaiko sokoZaiko) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko sokoZaiko);
    }
}