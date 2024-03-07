using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Convenience.Models.Date.Chumon
{
    [Table("chumon_jisseki")]
    [PrimaryKey(nameof(ChumonId), nameof(ShiireSakiId))]
    public class ChumonJisseki
    {
        //注文コード
        [Column("chumon_code", TypeName = "character varying(20)")]
        [DisplayName("注文コード")]
        [Required]
        public string ChumonId { get; set; }
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
        [DisplayName("仕入先コード")]
        [Required]
        public string ShiireSakiId { get; set; }
        //注文日
        [Column("chumon_date", TypeName = "date")]
        [DisplayName("注文日")]
        [Required]
        public DateOnly ChumonDate { get; set; }

        //仕入先マスタ　外部キーの設定
        [ForeignKey(nameof(ShiireSakiId))]
        public ShiireSakiMaster? ShiireSakiMaster { get; set; }
        public List<ChumonJissekiMeisai>? ChumonJissekiMeisais { get; set; }
    }
}
