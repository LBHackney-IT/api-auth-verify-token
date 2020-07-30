using System.Collections.Generic;
using ApiAuthTokenGenerator.V1.Domain;

namespace ApiAuthTokenGenerator.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
