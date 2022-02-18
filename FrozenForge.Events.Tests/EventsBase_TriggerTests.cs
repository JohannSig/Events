using FrozenForge.Events;
using Xunit;

namespace Sovereign.Tests
{
    public class EventsBase_TriggerTests
	{
		public EventsBase_TriggerTests()
		{
			Events = new EventsBase();
		}

		public EventsBase Events { get; }

		[Fact]
		public void InvokesSubscribingMethod()
		{
			var isInvoked = false;

			Events.Register<TestEvent>(@event => isInvoked = true);

			Assert.False(isInvoked);

			Events.TriggerAsync(new TestEvent());

			Assert.True(isInvoked);
		}

		private class TestEvent { }

	}
}
