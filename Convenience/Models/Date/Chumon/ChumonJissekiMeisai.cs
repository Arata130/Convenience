using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Convenience.Models.Date.Shiire;

namespace Convenience.Models.Date.Chumon
{
    [Table("chumon_jisseki_meisai")]
    [PrimaryKey(nameof(ChumonId), nameof(ShiireSakiId), nameof(ShiirePrdId), nameof(ShohinId))]
    public class ChumonJissekiMeisai {
        //注文コード
        [Column("chumon_code", TypeName = "character varying(20)")]
        [Required]
        public string ChumonId { get; set; }
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
        [Display]
        [Required]
        public string ShiireSakiId { get; set; }
        //仕入商品コード
        [Column("shiire_prd_code", TypeName = "character varying(10)")]
        [Required]
        public string ShiirePrdId { get; set; }
        //商品コード
        [Column("shohin_code", TypeName = "character varying(10)")]
        [Required]
        public string ShohinId { get; set; }
        //注文数
        [Column("chumon_su", TypeName = "numeric(10, 2)")]
        [Required]
        [Precision(10, 2)]
        public decimal ChumonSu { get; set; }
        //注文残
        [Column("chumon_zan", TypeName = "numeric()10, 2")]
        [Required]
        [Precision(10, 2)]
        public decimal ChumonZan { get; set; }

        //注文実績 外部キーの設定
        [ForeignKey(nameof(ChumonId) + "," + nameof(ShiireSakiId))]
        public ChumonJisseki? ChumonJisseki { get; set; }
        //仕入マスタ　外部キーの設定
        [ForeignKey(nameof(ShiireSakiId) + "," + nameof(ShiirePrdId) + "," + nameof(ShohinId))]
        public ShiireMaster? ShiireMaster { get; set; }
        [ForeignKey(nameof(ChumonId) + "," + nameof(ShiireSakiId) + "," + nameof(ShiirePrdId) + "," + nameof(ShiireDate) + "," + nameof(SeqByShiireDate))]
        public ShiireJisseki? ShiireJisseki { get; set; }


        [NotMapped]
        public decimal LastChumonSu { get; internal set; }
        [NotMapped]
        public DateTime ShiireDate { get; set; }
        [NotMapped]
        public uint SeqByShiireDate { get; set; }

    }
}
