﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Composable.CQRS.EventSourcing;
using Composable.CQRS.EventSourcing.EventRefactoring.Migrations;
using Composable.CQRS.EventSourcing.SQLServer;
using Composable.CQRS.Windsor;
using Composable.CQRS.Windsor.Testing;
using Composable.ServiceBus;
using Composable.System.Linq;
using Composable.UnitsOfWork;
using FluentAssertions;
using NCrunch.Framework;
using NUnit.Framework;
using TestAggregates;
using TestAggregates.Events;


namespace CQRS.Tests.CQRS.EventSourcing.EventRefactoring.Migrations
{
    public class InMemoryEventStoreEventStreamMutatorTests : EventStreamMutatorTests
    {
        public InMemoryEventStoreEventStreamMutatorTests() : base(typeof(InMemoryEventStore)) { }
    }

    public class SqlServerEventStoreEventStreamMutatorTests : EventStreamMutatorTests
    {
        public SqlServerEventStoreEventStreamMutatorTests() : base(typeof(SqlServerEventStore)) { }
    }

    [TestFixture, ExclusivelyUses(NCrunchExlusivelyUsesResources.EventStoreDbMdf)]
    public abstract class EventStreamMutatorTests : EventStreamMutatorTestsBase
    {
        protected EventStreamMutatorTests(Type eventStoreType) : base(eventStoreType) {}
        [Test]
        public void Replacing_E1_with_E2()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef, Ef>(),
                Seq.OfTypes<Ec1, E2, Ef, Ef>(),
                ReplaceEventType<E1>.With<E2>());
        }

        [Test]
        public void Replacing_E1_with_E2_E3()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E2, E3, Ef>(),
                ReplaceEventType<E1>.With<E2, E3>());
        }

        [Test]
        public void Replacing_E1_with_E2_E3_2()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef, Ef, Ef, Ef>(),
                Seq.OfTypes<Ec1, E2, E3, Ef, Ef, Ef, Ef>(),
                ReplaceEventType<E1>.With<E2, E3>());
        }

        [Test]
        public void Replacing_E1_with_E2_then_irrelevant_migration()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E2, Ef>(),
                ReplaceEventType<E1>.With<E2>(),
                ReplaceEventType<E1>.With<E5>());
        }

        [Test]
        public void Replacing_E1_with_E2_E3_then_an_unrelated_migration_v2()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E2, E3, Ef>(),
                ReplaceEventType<E1>.With<E2, E3>(),
                ReplaceEventType<E1>.With<E5>());
        }

        [Test]
        public void Replacing_E1_with_E2_E3_then_E2_with_E4()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E4, E3, Ef>(),
                ReplaceEventType<E1>.With<E2,E3>(),
                ReplaceEventType<E2>.With<E4>());
        }


        [Test]
        public void Inserting_E3_before_E1()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E3, E1, Ef>(),
                BeforeEventType<E1>.Insert<E3>());
        }

        [Test]
        public void Inserting_E3_E4_before_E1()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E3, E4, E1, Ef>(),
                BeforeEventType<E1>.Insert<E3, E4>());
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E3()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1>(),
                Seq.OfTypes<Ec1, E5, E3, E4, E1>(),
                BeforeEventType<E1>.Insert<E3, E4>(),
                BeforeEventType<E3>.Insert<E5>());
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E4()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E3, E5, E4, E1, Ef>(),
                BeforeEventType<E1>.Insert<E3, E4>(),
                BeforeEventType<E4>.Insert<E5>());
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E3_2()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef, Ef>(),
                Seq.OfTypes<Ec1, E5, E3, E4, E1, Ef, Ef>(),
                BeforeEventType<E1>.Insert<E3, E4>(),
                BeforeEventType<E3>.Insert<E5>());
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E4_2()
        {
            RunMigrationTest(
                Seq.OfTypes<Ec1, E1, Ef, Ef>(),
                Seq.OfTypes<Ec1, E3, E5, E4, E1, Ef, Ef>(),
                BeforeEventType<E1>.Insert<E3, E4>(),
                BeforeEventType<E4>.Insert<E5>());
        }

        [Test]
        public void A_one_thousand_events_large_aggregate_with_four_migrations_should_load_cached_in_less_than_50_milliseconds()
        {
            var eventMigrations = Seq.Create<IEventMigration>(
                BeforeEventType<E6>.Insert<E2>(),
                BeforeEventType<E7>.Insert<E3>(),
                BeforeEventType<E8>.Insert<E4>(),
                BeforeEventType<E9>.Insert<E5>()
                ).ToArray();

            var container = CreateContainerForEventStoreType(eventMigrations, EventStoreType);

            var history = Seq.OfTypes<Ec1>().Concat(1.Through(1000).Select(index => typeof(E1))).ToArray();
            var aggregate = TestAggregate.FromEvents(Guid.NewGuid(), history);
            container.ExecuteUnitOfWorkInIsolatedScope(() => container.Resolve<IEventStoreSession>().Save(aggregate));

            //Warm up cache..
            var watch = new Stopwatch();
            watch.Start();
            container.ExecuteUnitOfWorkInIsolatedScope(() => container.Resolve<IEventStoreSession>().Get<TestAggregate>(aggregate.Id));
            var elapsed = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsed);
            watch.Reset();

            watch.Start();

            var timesToLoadAggregate = 10;
            for(int iterations = 0; iterations < timesToLoadAggregate; iterations++)
            {
                container.ExecuteUnitOfWorkInIsolatedScope(() => container.Resolve<IEventStoreSession>().Get<TestAggregate>(aggregate.Id));
            }

            elapsed = watch.ElapsedMilliseconds;
            var millisecondsPerLoad = (double)elapsed / timesToLoadAggregate;
            Console.WriteLine(millisecondsPerLoad);
            millisecondsPerLoad.Should().BeLessOrEqualTo(50);

        }
    }
}