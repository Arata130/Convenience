using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Convenience.Models.Date.Chumon;

namespace Convenience.Models.Date.Shiire {
    [Table("soko_zaiko")]
    [PrimaryKey(nameof(ShiireSakiId), nameof(ShiirePrdId), nameof(ShohinId))]
    public class SokoZaiko {
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
        [Required]
        public string ShiireSakiId { get; set; }
        //仕入商品コード
        [Column("soko_zaiko", TypeName = "character varying(10)")]
        [Required]
        public string ShiirePrdId { get; set; }
        //商品コード
        [Column("shohin_code", TypeName = "character varying(10)")]
        [Required]
        public string ShohinId { get; set; }
        //仕入単位在庫数
        [Column("soko_zaiko_case_su", TypeName = "numeric(10, 2)")]
        [Required]
        [Precision(10, 2)]
        public decimal SokoZaikoCaseSu { get; set; }
        //在庫数
        [Column("soko_zaiko_su", TypeName = "numeric(10, 2)")]
        [Required]
        [Precision(10, 2)]
        public decimal SokoZaikoSu { get; set; }
        //直近仕入日
        [Column("last_shiire_date", TypeName = "date")]
        [Required]
        public DateOnly LastShiireDate { get; set; }
        //直近払出日
        [Column("last_delivery_date", TypeName = "date")]
        public DateOnly LastDeliveryDate { get; set; }

        [ForeignKey(nameof(ShiireSakiId) + "," + nameof(ShiirePrdId) + "," + nameof(ShohinId))]
        public ShiireMaster ShiireMaster { get; set; }


    }
}
