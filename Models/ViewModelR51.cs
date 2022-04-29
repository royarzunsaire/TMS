using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelR51
    {
        public R51 r51 { get; set; }
        public IEnumerable<Checklist> checklists { get; set; }
        public IEnumerable<R51_Checklist> r51_Checklists { get; set; }
        public IEnumerable<Ciudad> ciudad { get; set; }

    }
}