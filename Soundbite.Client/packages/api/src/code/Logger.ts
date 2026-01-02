// Enumeration of the various log levels
enum LogLevel {
  trace = 0,
  info = 1,
  warning = 2,
  error = 4,
}

/**
 * Class for logging application runtime information
 */
class LoggerClass {
  /**
   * Responsible for applying all logging logic
   * @param level - log level associated with the message
   * @param msg - message to log
   * @param ex - error information (optional)
   */
  private Log(level: LogLevel, msg: string, ex?: any): void {
    // Acquire current log level
    const minLogLevel: number = (window as any).sbMinLogLevel | LogLevel.trace;

    // Determine whether to log the message
    if (level >= minLogLevel) {
      if (ex) {
        if (level >= LogLevel.error) {
          console.error(`[SB]-${msg}`, ex);
        } else if (level >= LogLevel.warning) {
          console.warn(`[SB]-${msg}`, ex);
        } else {
          console.log(`[SB]-${msg}`, ex);
        }
      } else {
        if (level >= LogLevel.error) {
          console.error(`[SB]-${msg}`);
        } else if (level >= LogLevel.warning) {
          console.warn(`[SB]-${msg}`);
        } else {
          console.log(`[SB]-${msg}`);
        }
      }
    }
  }

  /**
   * Log a trace level message
   * @param msg - message to log
   */
  LogTrace(msg: string) {
    this.Log(LogLevel.trace, msg);
  }

  /**
   * Log an informational level message
   * @param msg - message to log
   */
  LogInfo(msg: string) {
    this.Log(LogLevel.info, msg);
  }

  /**
   * Log a warning level message
   * @param msg - message to log
   */
  LogWarning(msg: string) {
    this.Log(LogLevel.warning, msg);
  }

  /**
   * Log an error level message
   * @param msg - message to log
   * @param error - error information
   */
  LogError(msg: string, ex?: any) {
    this.Log(LogLevel.error, msg, ex);
  }
}

export const Logger = new LoggerClass();
