namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using AutoFixture.Idioms;

    /// <summary>
    /// Encapsulates the expected behavior when an <see cref="IGuardClauseCommand" /> (typically
    /// representing a method or constructor) is invoked with a <see cref="Double" /> less than 0 argument.
    /// </summary>
    /// <seealso cref="Verify(IGuardClauseCommand)" />
    public class NegativeDoubleBehaviorExpectation : IBehaviorExpectation
    {
        /// <summary>
        /// Verifies the behavior of the command when invoked with <see cref="Double" /> less than 0.
        /// </summary>
        /// <param name="command">The command whose behavior must be examined.</param>
        /// <remarks>
        /// <para>
        /// This method encapsulates the behavior which is expected when a method or constructor is
        /// invoked with <see cref="Double" /> less than 0 as one of the method arguments. In that case it's
        /// expected that invoking <paramref name="command" /> with Double less than 0 throws an
        /// <see cref="ArgumentException" />, causing the Verify method to succeed. If other
        /// exceptions are thrown, or no exception is thrown when invoking the command, the Verify
        /// method throws an exception.
        /// </para>
        /// </remarks>
        public void Verify(IGuardClauseCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.RequestedType != typeof(double))
                return;

            try
            {
                command.Execute(Convert.ToDouble(new Random().Next(int.MinValue, -1)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            catch (Exception e)
            {
                throw command.CreateException("\"Double.Negative\"", e);
            }

            throw command.CreateException("\"Double.Negative\"");
        }
    }
}
