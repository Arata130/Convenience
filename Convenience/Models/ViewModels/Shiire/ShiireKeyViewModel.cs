using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Convenience.Models.ViewModels.Shiire {
    public class ShiireKeyViewModel {
        [Column("chumon_code")]
        [DisplayName("注文コード")]
        [MaxLength(10)]
        [Required]
        public string ChumonId { get; set; }

        public IList<SelectListItem> ChumonIdList { get; set; }
    }
}

