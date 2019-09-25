using IOT.Demo.Umbraco.RequestHandlers;
using System;
using System.Web.Http;
using Umbraco.Core.Composing;

namespace IOT.Demo.Umbraco.Components
{
    public class RegisterHandlerComponent : IComponent
    {
        public RegisterHandlerComponent()
        {

        }

        public void Initialize()
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(new AlexaRequestValidationHandler());
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}