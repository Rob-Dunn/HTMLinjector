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
    { }

    /// <summary>
    /// Used to communicate build progress and errors
    /// </summary>
    public class BuildHandler
    {
        public event EventHandler<BuildEventArgs> BuildEvent;

        /// <summary>
        /// Gets or sets a value indicating whether a build error has occurred
        /// </summary>
        /// <value><c>true</c> if has error; otherwise, <c>false</c>.</value>
        public bool HasError { get; internal set; }

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

            throw new BuildException();
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

            throw new BuildException();
        }
    }
}
