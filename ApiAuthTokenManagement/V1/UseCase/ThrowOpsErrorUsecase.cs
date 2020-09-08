using ApiAuthTokenManagement.V1.Domain.Exceptions;

namespace ApiAuthTokenManagement.V1.UseCase
{
    public static class ThrowOpsErrorUsecase
    {
        public static void Execute()
        {
            throw new TestOpsErrorException();
        }
    }
}
