using Convenience.Models.Date.Shiire;

namespace Convenience.Models.Interfaces {

    public interface IShiire {
        public List<ShiireJisseki> ShiireJissekis { get; set; }
        public List<SokoZaiko> SokoZaikos { get; set; }
        public ShiireJisseki ShiireToiawase(string inChumonId);
        public List<ShiireJisseki> ShiireCreate(string inChumonId);
        public List<SokoZaiko> ZaikoCreate(string inChumonId);
        public ShiireJisseki ChumonZanBalance(ShiireJisseki inshiireJisseki);
        public (ShiireJisseki shiireJisseki, SokoZaiko sokoZaiko) ShiireJissekiUpdate(ShiireJisseki inshiireJisseki, SokoZaiko sokoZaiko);
    }
}