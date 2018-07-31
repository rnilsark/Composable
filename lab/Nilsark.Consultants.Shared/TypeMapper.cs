using Composable.Refactoring.Naming;

namespace Nilsark.Consultants.Shared
{
    public static class TypeMapper
    {
        public static void MapTypes(ITypeMappingRegistar typeMapper)
        {
            typeMapper
                .Map<Commands.EnrolConsultantCommand>("AD91865F-1A63-499E-92EE-73B08131838E")
                .Map<Commands.CommandResult>("82483980-61ED-42B2-A8FD-AE30D1444924")
                .Map<Domain.Events.IConsultantEvent>("8219d6ce-f0b5-4c1f-801a-81f774f86530")
                .Map<Domain.Events.IConsultantEnrolledEvent>("a54262a6-8c04-48c6-8992-25fda1fbb3bb")
                .Map<Domain.Events.IConsultantEmailPropertyUpdated>("3b892126-9da7-458c-9989-8dcae8e6550b")
                .Map<Domain.Events.IConsultantFullNamePropertyUpdated>("d7f41dd2-a178-4b6a-a2ea-9ac3e5825048")
                .Map<QueryModels.ConsultantIdsQueryModel>("197bca16-c0e1-42c5-8f46-d9fdb94804f6")
                .Map<Queries.GetConsultantIdsQuery>("51f8ac95-44a1-45bf-89ec-aa612475ac1e")
                .Map<Queries.GetConsultantQuery>("75b32fd1-1dc7-444a-b4b9-962c5a903537")
                .Map<Domain.Events.ConsultantEnrolled>("3ce51829-88b5-4cc7-a49a-f70a43fb3c55")
                .Map<Domain.Events.ConsultantEvent>("7b0efe73-db96-4553-96b5-dbb70e7a7546")
                .Map<QueryModels.ConsultantQueryModel>("5eef7301-27f6-4b35-94a3-41a0393f7368")
                .Map<Domain.Events.IConsultantDismissedEvent>("cd1bae0d-fa31-4be7-b26a-b1edbe891e74")
                .Map<Domain.Events.ConsultantDismissed>("f078c182-89ca-4348-ba20-02d652e83263")
                .Map<Commands.DismissConsultantCommand>("2EA3B4E1-1A28-4D63-ADB9-5413D3059BD5");
        }
    }
}