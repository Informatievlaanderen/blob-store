namespace Be.Vlaanderen.Basisregisters.BlobStore.Framework
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;

    /// <summary>
    /// Encapsulates the expected behavior when an <see cref="IGuardClauseCommand" /> (typically
    /// representing a method or constructor) is invoked with a <see cref="Int32" /> less than 0 argument.
    /// </summary>
    /// <seealso cref="Verify(IGuardClauseCommand)" />
    public class NegativeInt32BehaviorExpectation : IBehaviorExpectation
    {
        private readonly int[] _exceptions;

        public NegativeInt32BehaviorExpectation()
        {
            _exceptions = new int[0];
        }

        public NegativeInt32BehaviorExpectation(params int[] exceptions)
        {
            _exceptions = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
        }

        /// <summary>
        /// Verifies the behavior of the command when invoked with <see cref="Int32" /> less than 0.
        /// </summary>
        /// <param name="command">The command whose behavior must be examined.</param>
        /// <remarks>
        /// <para>
        /// This method encapsulates the behavior which is expected when a method or constructor is
        /// invoked with <see cref="Int32" /> less than 0 as one of the method arguments. In that case it's
        /// expected that invoking <paramref name="command" /> with Int32 less than 0 throws an
        /// <see cref="ArgumentException" />, causing the Verify method to succeed. If other
        /// exceptions are thrown, or no exception is thrown when invoking the command, the Verify
        /// method throws an exception.
        /// </para>
        /// </remarks>
        public void Verify(IGuardClauseCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.RequestedType != typeof(int))
                return;

            try
            {
                var random = new Random();
                if (_exceptions.Length != 0)
                {
                    var value = random.Next(int.MinValue, -1);
                    while (_exceptions.Contains(value))
                    {
                        value = random.Next(int.MinValue, -1);
                    }
                    command.Execute(value);
                }
                else
                {
                    command.Execute(random.Next(int.MinValue, -1));
                }

            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            catch (Exception e)
            {
                throw command.CreateException("\"Int32.Negative\"", e);
            }

            throw command.CreateException("\"Int32.Negative\"");
        }
    }
}
