using System;
namespace HTMLinjectorServices
{
    public interface IBuildHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether a build error has occurred
        /// </summary>
        /// <value>True when there was an error</value>
        bool HasError { get; set; }

        /// <summary>
        /// Tells any listeners that a build event has occured
        /// </summary>
        /// <param name="eventMessage">The build event text</param>
        void SignalBuildEvent(string eventMessage);

        /// <summary>
        /// Tells listeners that a build error occurred
        /// </summary>
        /// <param name="eventMessage">The build error text</param>
        void SignalBuildError(string eventMessage);

        /// <summary>
        /// Tells listeners that a build exception occurred
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        void SignalBuildException(Exception exception);
    }
}
