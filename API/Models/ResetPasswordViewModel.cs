﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class ResetPasswordViewModel
    {      
        public string userName { get; set; }
      
        public string password { get; set; }
       
    }
}
