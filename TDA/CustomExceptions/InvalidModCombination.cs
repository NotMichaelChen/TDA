using System;
using System.Runtime.Serialization;

namespace CustomExceptions
{
    /// Thrown when a given combination of mods is invalid
    public class InvalidModCombination : Exception, ISerializable
    {
        public InvalidModCombination()
        {
        }

        public InvalidModCombination(string message) : base(message)
        {
        }

        public InvalidModCombination(string message, Exception innerException) : base(message, innerException)
        {
        }

        // This constructor is needed for serialization.
        protected InvalidModCombination(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}