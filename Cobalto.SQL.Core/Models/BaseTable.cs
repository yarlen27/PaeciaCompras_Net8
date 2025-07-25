using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cobalto.SQL.Core.Models
{
    //public class BaseTable
    //{

    //    public int Id { get; set; }
    //    public bool Borrado { get; set; }
    //}

    public class GenericTable
    {
        //[System.ComponentModel.DataAnnotations.Key]
        //[Required]
        //public T Id { get; set; }

        public bool Borrado { get; set; }
    }

    public class BaseTable : GenericTable
    {

        //[System.ComponentModel.DataAnnotations.Key]
        //[Required]
        public int Id { get; set; }


    }


    public class BaseStringTable : GenericTable
    {

        [System.ComponentModel.DataAnnotations.Key]
        [Required]
        public string Id { get; set; }

    }
}
