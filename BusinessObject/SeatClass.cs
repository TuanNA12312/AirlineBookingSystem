using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class SeatClass
    {
        [Key]
        public int SeatClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClassName { get; set; } = "Economy";
    }
}
