using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Models.CustomModels
{
    public  class AlertPopupModel(string title, string description)
    {
        public string Title { get; set; } = title;
        public string Description { get; set; } = description;
    }
}
