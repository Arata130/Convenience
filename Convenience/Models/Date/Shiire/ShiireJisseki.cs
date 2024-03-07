using Convenience.Models.Date.Chumon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Convenience.Models.Date.Shiire {
    [Table("shiire_jisseki")]
    [PrimaryKey(nameof(ChumonId), nameof(ShiireDate), nameof(SeqByShiireDate), nameof(ShiireSakiId), nameof(ShiirePrdId))]
    public class ShiireJisseki {
        //注文コード
        [Column("chumon_code", TypeName = "character varying(20)")]
        [DisplayName("注文コード")]
        [Required]
        public string ChumonId { get; set; }
        //仕入日付
        [Column("shiire_date", TypeName = "date")]
        [DisplayName("仕入日")]
        [Required]
        public DateOnly ShiireDate { get; set; }
        //仕入SEQ
        [Column("seq_by_shiiredate", TypeName = "bigint")]
        [DisplayName("仕入SEQ")]
        [Required]
        public uint SeqByShiireDate { get; set; }
        //仕入日時
        [Column("shiire_datetime", TypeName = "timestamp with time zone")]
        [DisplayName("仕入日時")]
        [Required]
        public DateTime ShiireDateTime { get; set; }
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
        [DisplayName("仕入先コード")]
        [Required]
        public string ShiireSakiId { get; set; }
        //仕入商品コード
        [Column("shiire_prd_code", TypeName = "character varying(10)")]
        [DisplayName("仕入商品コード")]
        [Required]
        public string ShiirePrdId { get; set; }
        //商品コード
        [Column("shohin_code", TypeName = "character varying(10)")]
        [DisplayName("商品コード")]
        [Required]
        public string ShohinId { get; set; }
        //納入数
        [Column("nonyu_su", TypeName = "numeric(10, 2)")]
        [DisplayName("納入数")]
        [Required]
        [Precision(10, 2)]
        public decimal NonyuSu { get; set; }

        [ForeignKey(nameof(ChumonId) + "," + nameof(ShiireSakiId) + "," + nameof(ShiirePrdId) + "," + nameof(ShohinId))]
        public ChumonJissekiMeisai? ChumonJissekiMeisais { get; set; }
    }
}
