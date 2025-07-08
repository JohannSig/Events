using Xunit;

namespace FrozenForge.Events.Tests
{
    public class EventsBase_DisposeTests
    {
        public EventsBase_DisposeTests()
        {
            Events = new EventsBase();
        }

        public EventsBase Events { get; }


        [Fact]
        public void DisposesItsEventContainers()
        {
            var timesDisposeCalledForContainer1 = 0;
            var timesDisposeCalledForContainer2 = 0;

            var container1 = new EventRegistrationContainer<TestEvent>();
            container1.OnDisposed += x => timesDisposeCalledForContainer1++;

            var container2 = new EventRegistrationContainer<TestEvent2>();
            container2.OnDisposed += x => timesDisposeCalledForContainer2++;

            Events.RegistrationContainerByType.Add(typeof(TestEvent), container1);
            Events.RegistrationContainerByType.Add(typeof(TestEvent2), container2);

            Assert.Equal(0, timesDisposeCalledForContainer1);
            Assert.Equal(0, timesDisposeCalledForContainer2);

            Events.Dispose();

            Assert.Equal(1, timesDisposeCalledForContainer1);
            Assert.Equal(1, timesDisposeCalledForContainer2);
        }

        [Fact]
        public void IsOnlyDisposedOnceRegardlessOfAmountOfEventContainers()
        {
            Events.Register<TestEvent>(@event => { });
            Events.Register<TestEvent2>(@event => { });

            var container1 = Events.RegistrationContainerByType[typeof(TestEvent)];
            var container2 = Events.RegistrationContainerByType[typeof(TestEvent2)];

            Events.Dispose();
        }

        private class TestEvent { }
        private class TestEvent2 { }
    }

}
