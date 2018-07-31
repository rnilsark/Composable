using Composable.Refactoring.Naming;

namespace Nilsark.Consultants.Server
{
    internal static class TypeMapper
    {
        internal static void MapTypes(ITypeMappingRegistar typeMapper)
        {
            typeMapper
                .Map<Domain.Consultant>("E843DFB2-2A72-46E5-89F5-2B3BA1FE3889");

        }
    }
}