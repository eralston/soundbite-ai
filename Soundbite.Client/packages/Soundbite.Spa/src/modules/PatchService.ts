/**
 * Enumeration of the various HTTP PATCH Verb operations
 **/
export enum PatchOperationType {
  none = "none",
  add = "add",
  copy = "copy",
  move = "move",
  remove = "remove",
  replace = "replace",
  test = "test",
}

/**
 * Represents an individual patch operation to apply to a data object.
 **/
export interface IPatchOperation {
  op: PatchOperationType; // Specifies the operation to run (required)
  path: string; // Specifies the location at which the operation runs (required)
  from?: string; // Specified the source location in a Copy or Move operation
  value?: any; // Specifies the value in a Replace or Test operation
}

/**
 * Responsible for generating a patch document (i.e. array of patch operations) given the original
 * state of an object and the updated state.  This is a very naive approach that only operates on
 * the changes directly to existing properties of an object.  It DOES NOT handle array changes
 * or any advanced patching scenarios.
 **/
export class PatchService {
  generatePatch(objOriginal: any, objUpdated: any): IPatchOperation[] {
    let result: IPatchOperation[] = [];

    for (const propName in objUpdated) {
      if (objOriginal[propName] !== objUpdated[propName]) {
        result.push({
          path: `/${propName}`,
          op: PatchOperationType.replace,
          value: objUpdated[propName],
        } as IPatchOperation);
      }
    }

    return result;
  }
}
