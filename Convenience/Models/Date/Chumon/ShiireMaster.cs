using Convenience.Models.Date.Shiire;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Convenience.Models.Date.Chumon
{
    [Table("shiire_master")]
    [PrimaryKey(nameof(ShiireSakiId), nameof(ShiirePrdId), nameof(ShohinId))]
    public class ShiireMaster
    {
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
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
        //仕入商品名称
        [Column("shiire_prd_name", TypeName = "character varying(30)")]
        [Required]
        public string ShiirePrdName { get; set; }
        //仕入単位における数量
        [Column("shiire_pcs_unit", TypeName = "numeric(7, 2)")]
        [Required]
        [Precision(15, 2)]
        public decimal ShiirePcsPerUnit { get; set; }
        //仕入単位
        [Column("shiire_unit", TypeName = "character varying(10)")]
        [Required]
        public string ShiireUnit { get; set; }
        //仕入単価
        [Column("shiire_tanka", TypeName = "numeric(7, 2)")]
        [Required]
        [Precision(15, 2)]
        public decimal ShireTanka { get; set; }

        //仕入先マスタ　外部キーの設定
        [ForeignKey(nameof(ShiireSakiId))]
        public ShiireSakiMaster? ShiireSakiMaster { get; set; }
        //商品マスタ　外部キーの設定
        [ForeignKey(nameof(ShohinId))]
        public ShohinMaster? ShohinMaster { get; set; }
        public List<ChumonJissekiMeisai>? ChumonJissekiMeisais { get; set; }
        public SokoZaiko? SokoZaikos { get; set; }

    }
}
