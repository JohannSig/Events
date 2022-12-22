using System;
using System.Linq;
using Xunit;

namespace FrozenForge.Events.Tests
{
    public class EventsBase_RegistrationTests
    {
        public EventsBase_RegistrationTests()
        {
            Events = new EventsBase();
        }

        public EventsBase Events { get; }

        [Fact]
        public void AddsEventContainerEntryToDictionaryIfNoneExists()
        {
            Action<TestEvent> onTestEvent = @event => { };

            Assert.Empty(Events.RegistrationContainerByType);

            var subscription = Events.Register(onTestEvent);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Single((Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations);
        }

        [Fact]
        public void AddsOneSubscriptionToEventContainer()
        {
            Action<TestEvent> onTestEvent = @event => { };

            Assert.Empty(Events.RegistrationContainerByType);

            var subscription = Events.Register(onTestEvent);

            Assert.Single((Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations);
        }

        [Fact]
        public void AddsSubscriptionToExistingEventContainer()
        {
            Action<TestEvent> onTestEvent1 = @event => { };
            Action<TestEvent> onTestEvent2 = @event => { };

            var subscription1 = Events.Register(onTestEvent1);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Single((Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations);

            var subscription2 = Events.Register(onTestEvent2);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Equal(2, (Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations.Count);
        }

        [Fact]
        public void ReusesEventContainerEntryForMultipleSubscribersOfSameType()
        {
            static void onTestEvent1(TestEvent @event) { }
            static void onTestEvent2(TestEvent @event) { }

            var subscription1 = Events.Register((Action<TestEvent>)onTestEvent1);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Single((Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations);

            var subscription2 = Events.Register((Action<TestEvent>)onTestEvent2);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Equal(2, (Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations.Count);
        }

        [Fact]
        public void AddsOrReusesEventContainerEntryDependingOnEventType()
        {
            static void onTestEvent1(TestEvent @event) { }
            static void onTestEvent2(TestEvent @event) { }
            static void onTestEvent3(TestEvent2 @event) { }

            Events.Register((Action<TestEvent>)onTestEvent1);

            Assert.Single(Events.RegistrationContainerByType);
            Assert.Single((Events.RegistrationContainerByType.Single().Value as EventRegistrationContainer<TestEvent>).Registrations);

            Events.Register((Action<TestEvent>)onTestEvent2);
            Events.Register((Action<TestEvent2>)onTestEvent3);

            Assert.Equal(2, Events.RegistrationContainerByType.Count);
            Assert.Equal(2, (Events.RegistrationContainerByType.First().Value as EventRegistrationContainer<TestEvent>).Registrations.Count);
            Assert.Equal(1, (Events.RegistrationContainerByType.Skip(1).First().Value as EventRegistrationContainer<TestEvent2>).Registrations.Count);
        }

        private class TestEvent { }
        private class TestEvent2 { }

    }
}
