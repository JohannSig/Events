using System;

namespace FrozenForge.Events.Exceptions;

internal class EventRegistrationException(string message) : Exception(message)
{
}