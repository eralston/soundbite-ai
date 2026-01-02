import { PublicError } from "@soundbite/api";
import { ErrorDlg } from "@soundbite/widgets-react";

// Based catch-decorator by Enkot https://github.com/enkot/catch-decorator

/**
 * Decorator factory that wraps the exceptions of a method and causes the ErrorDlg to show the exception to the user
 */
export default (name: string, message?: string): any => {
  return (target: any, propertyKey: string, descriptor: PropertyDescriptor) => {
    // save a reference to the original method
    const originalMethod = descriptor.value;

    // rewrite original method with custom wrapper
    descriptor.value = function (...args: any[]) {
      try {
        const result = originalMethod.apply(this, args);

        // check if method is asynchronous
        if (
          result &&
          typeof result.then === "function" &&
          typeof result.catch === "function"
        ) {
          // return promise
          return result.catch((error: any) => {
            // Wrap error in PublicError and show to dialog
            if (!(error instanceof PublicError))
              error = new PublicError(error, name, message);
            ErrorDlg.show(error);
          });
        }

        // return actual result
        return result;
      } catch (error: any) {
        let err = error;
        // Wrap error in PublicError and show in dialog
        if (!(err instanceof PublicError))
          err = new PublicError(error, name, message);
        ErrorDlg.show(error);
      }
    };

    return descriptor;
  };
};
