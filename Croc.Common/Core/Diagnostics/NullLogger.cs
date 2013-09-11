namespace Croc.Core.Diagnostics 

{ 

	/// <summary> 

    /// ?????????? ILogger, ??????? ?????? ?? ??????. 

	/// </summary> 

	internal class NullLogger : ILogger 

	{ 

        /// <inheritdoc/> 

        public void Log(LoggerEvent logEvent) 

		{ 

		} 

 

 

        /// <inheritdoc/> 

        public bool IsAcceptedByEventType(LoggerEvent logEvent) 

        { 

            return false; 

        } 

	} 

}


