using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels;

public class TodoPutViewModel : TodoPostViewModel
{
    public bool Complete { get; set; }
    
}
