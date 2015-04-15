namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    internal interface IMethodHandler
    {
        //The method that process the requests
        HandlerResult Handle();
    }
}
