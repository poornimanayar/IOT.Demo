using IOT.Demo.Umbraco.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace IOT.Demo.Umbraco.Composers
{
    public class RegisterHandlerComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<RegisterHandlerComponent>();
        }
    }
}
