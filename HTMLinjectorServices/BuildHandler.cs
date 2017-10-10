using System;
namespace HTMLinjectorServices
{
    public class BuildEventArgs : EventArgs
    {
        public string EventMessage { get; set; }
    }

    /// <summary>
    /// Represents a build exception
    /// </summary>
    public class BuildException : Exception
    { 
        public BuildException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to communicate build progress and errors
    /// </summary>
    public class BuildHandler : IBuildHandler
    {
        public event EventHandler<BuildEventArgs> BuildEvent;

        /// <summary>
        /// Gets or sets a value indicating whether a build error has occurred
        /// </summary>
        /// <value>True when there was an error</value>
        public bool HasError { get; set; }

        /// <summary>
        /// Tells any listeners that a build event has occured
        /// </summary>
        /// <param name="eventMessage">The build event text</param>
        public void SignalBuildEvent(string eventMessage)
        {
            if (this.BuildEvent != null)
            {
                this.BuildEvent(null, new BuildEventArgs() { EventMessage = eventMessage });
            }
        }

        /// <summary>
        /// Tells listeners that a build error occurred
        /// </summary>
        /// <param name="eventMessage">The build error text</param>
        public void SignalBuildError(string eventMessage)
        {
            this.HasError = true;

            this.SignalBuildEvent(eventMessage);

            throw new BuildException(eventMessage);
        }

        /// <summary>
        /// Tells listeners that a build exception occurred
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        public void SignalBuildException(Exception exception)
        {
            this.HasError = true;

            string eventMessage = exception.Message;

            this.SignalBuildEvent(eventMessage);

            throw new BuildException(eventMessage);
        }
    }
}
