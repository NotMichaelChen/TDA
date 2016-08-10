using System;
using System.Runtime.Serialization;

namespace CustomExceptions
{
    /// Thrown when the score of a map (300/100/miss/maxcombo) is invalid
    public class InvalidScore : Exception, ISerializable
    {
        public InvalidScore()
        {
        }

        public InvalidScore(string message) : base(message)
        {
        }

        public InvalidScore(string message, Exception innerException) : base(message, innerException)
        {
        }

        // This constructor is needed for serialization.
        protected InvalidScore(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}