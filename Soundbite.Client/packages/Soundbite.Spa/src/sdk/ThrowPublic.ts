import { PublicError } from "@soundbite/api";

// Based catch-decorator by Enkot https://github.com/enkot/catch-decorator

// decorator factory function
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
            // TODO
            if (error instanceof PublicError) throw error;
            throw new PublicError(error, name, message);
          });
        }

        // return actual result
        return result;
      } catch (error: any) {
        // TODO
        if (error instanceof PublicError) throw error;

        throw new PublicError(error, name, message);
      }
    };

    return descriptor;
  };
};
