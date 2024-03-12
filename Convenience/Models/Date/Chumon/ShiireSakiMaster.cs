using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Convenience.Models.Date.Chumon {
    [Table("shiire_saki_master")]
    [PrimaryKey(nameof(ShiireSakiId))]
    public class ShiireSakiMaster
    {
        //仕入先コード
        [Column("shiire_saki_code", TypeName = "character varying(10)")]
        [DisplayName("仕入商品コード")]
        [Required]
        public string ShiireSakiId { get; set; }
        //仕入先会社
        [Column("shiire_saki_kaisya", TypeName = "character varying(30)")]
        [DisplayName("仕入先会社")]
        [Required]
        public string ShiireSakiKaisya { get; set; }
        //仕入先部署
        [Column("shiire_saki_busho", TypeName = "character varying(30)")]
        [DisplayName("仕入先部署")]
        [Required]
        public string ShiireSakiBusho { get; set; }
        //郵便番号
        [Column("yubin_bango", TypeName = "character varying(30)")]
        [DisplayName("郵便番号")]
        [Required]
        public string YubinBango { get; set; }
        //都道府県
        [Column("todoufuken", TypeName = "character varying(10)")]
        [DisplayName("都道府県")]
        [Required]
        public string Todoufuken { get; set; }
        //市区町村
        [Column("shikuchoson", TypeName = "character varying(10)")]
        [DisplayName("市区町村")]
        [Required]
        public string Shikuchoson { get; set; }
        //番地
        [Column("banchi", TypeName = "character varying(10)")]
        [DisplayName("番地")]
        [Required]
        public string Banchi { get; set; }
        //建物名
        [Column("tatemonomei", TypeName = "character varying(10)")]
        [DisplayName("建物名")]
        [Required]
        public string Tatemonomei { get; set; }

        public List<ShiireMaster>? ShiireMasters { get; set; }
        public List<ChumonJisseki>? ChumonJissekis { get; set; }
    }
}
