using FrozenForge.Events;
using Xunit;

namespace Sovereign.Tests
{
    public class EventRegistrationContainer_DisposeTests
	{

		[Fact]
		public void CallsConstructorOnDisposeMethod()
		{
			var isConstructorCallbackInvokedOnDispose = false;

			var container = new EventRegistrationContainer<TestEvent>();

			container.OnDisposed += x => isConstructorCallbackInvokedOnDispose = true;

			Assert.False(isConstructorCallbackInvokedOnDispose);

			container.Dispose();

			Assert.True(isConstructorCallbackInvokedOnDispose);
		}


		[Fact]
		public void AvoidsCircularDisposal()
		{
			var scopedEvents = new EventsBase();

			scopedEvents.Register<TestEvent>(@event => { });
			scopedEvents.Register<TestEvent>(@event => { });
			scopedEvents.Register<TestEvent>(@event => { });
			scopedEvents.Register<TestEvent>(@event => { });

			var container = scopedEvents.RegistrationContainerByType[typeof(TestEvent)];

			container.Dispose();
		}

		private class TestEvent { }
	}
}
