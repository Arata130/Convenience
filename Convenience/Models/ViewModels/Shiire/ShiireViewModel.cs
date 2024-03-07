using Convenience.Models.Date.Chumon;
using Convenience.Models.Date.Shiire;

namespace Convenience.Models.ViewModels.Shiire {
    public class ShiireViewModel {
        public List<ShiireJisseki> ShiireJisseki { get; set; }
        public List<SokoZaiko> SokoZaiko { get; set; }
        public string Remark { get; set; } = string.Empty;
    }
}
