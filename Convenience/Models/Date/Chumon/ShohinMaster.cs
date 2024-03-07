using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Convenience.Models.Date.Chumon
{
    [Table("shohin_master")]
    [PrimaryKey(nameof(ShohinId))]
    public class ShohinMaster
    {
        //商品コード
        [Column("shohin_code", TypeName = "character varying(10)")]
        [Required]
        public string ShohinId { get; set; }
        //商品名称
        [Column("shohin_name", TypeName = "character varying(50)")]
        [Required]
        public string ShohinName { get; set; }
        //消費税率
        [Column("shohi_zeiritsu", TypeName = "numeric(15, 2)")]
        [Required]
        [Precision(15, 2)]
        public decimal ShohiZeiritsu { get; set; }
        //消費税率（外食）
        [Column("shohi_zeiritsu_gaisyoku", TypeName = "numeric(15, 2)")]
        [Required]
        [Precision(15, 2)]
        public decimal ShohiZeiritsuGaishoku { get; set; }

        public List<ShiireMaster>? ShiireMasters { get; set; }
    }
}
