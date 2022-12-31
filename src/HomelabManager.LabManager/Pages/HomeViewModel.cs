using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLabManager.Manager.Pages
{
    public sealed class HomeViewModel : PageBaseViewModel
    {
        public override string Title => "Home";

        public override void Activate() { }

        public override bool TryDeactivate() => true;
    }
}
