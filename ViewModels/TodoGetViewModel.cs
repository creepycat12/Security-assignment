using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.ViewModels;

public class TodoGetViewModel
{
    public string Task { get; set; }
    public DateTime DueDate { get; set; }
    public bool Complete { get; set; }
}
