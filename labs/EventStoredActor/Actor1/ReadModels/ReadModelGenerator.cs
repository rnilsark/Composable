//using Actor1.Interfaces;
//using Common.CQRS;
//using Common.DDD;
//using Domain.Events;

//namespace Actor1.ReadModels
//{
//    public class ReadModelGenerator : ReadModelGenerator<IFooEvent, FooReadModel>
//    {
//        public ReadModelGenerator(IEventStreamReader eventStreamReader) : base(eventStreamReader)
//        {
//            RegisterEventAppliers()
//                .For<IFooNamePropertyUpdated>(e => ReadModel.Name = e.Name);
//        }
//    }
//}