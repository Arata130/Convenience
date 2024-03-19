using Convenience.Models.Date.Shiire;

namespace Convenience.Models.Interfaces {
    public interface ISokoZaiko { 
        public List<SokoZaiko> SokoZaiko { get; set; }
        public List<SokoZaiko> SokoZaikoSearch(bool SortByChumonZan, bool SortBySokoZaikoCaseSu, bool SortBySokoZaikoSu);
    }
}
