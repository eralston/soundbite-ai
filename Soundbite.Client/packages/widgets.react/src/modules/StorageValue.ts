/**
 * Utility class around window.sessionStorage that simplifies access and provides strong-typing
 * */
export default class StorageValue<ValueType> {
  private key: string;
  private storage: Storage;

  constructor(key: string, storage: Storage = sessionStorage) {
    this.key = key;
    this.storage = storage;
  }

  /**
   * Returns true if this has a value already; otherwise, false
   * */
  hasValue() {
    return this.storage.getItem(this.key) !== null;
  }

  /**
   * Removes the value from the underlying cache
   * */
  remove() {
    this.storage.removeItem(this.key);
  }

  /**
   * Retrieves the current value from session storage
   * */
  get(): ValueType | null {
    const json = this.storage.getItem(this.key);
    if (!json) return null;
    const value = JSON.parse(json) || null;
    return value;
  }

  /**
   * Sets the value in the underlying cache
   * @param value New value for this object to store in the cache
   */
  set(value: ValueType): void {
    const json = JSON.stringify(value);
    this.storage.setItem(this.key, json);
  }

  /**
   * Sets to the given value, but only is empty then this SessionValue object, making it chainable  during initialization
   * @param value Default value that is only set if empty
   */
  withDefault(value: ValueType): StorageValue<ValueType> {
    if (!this.hasValue()) this.set(value);
    return this;
  }
}

/**
 * Wrapper for localStorage providing typesafe and convenient behavior
 * */
export class LocalValue<ValueType> extends StorageValue<ValueType> {
  /**
   * Constructor for declaring a new instance of LocalValue
   * @param className The top-level namespace for building the key - recommend using the name of the class using localValue
   * @param valueName The name of the value for uniquely identifying it - recommend the name of the variable in the class
   * @param instanceName Unique name of the instance of this SessionValue - Defaults to "static" assuming there is only one value+class combo
   */
  constructor(
    className: string,
    valueName: string,
    instanceName: string = "static"
  ) {
    const key = `${className}::${instanceName}::${valueName}`;
    super(key, localStorage);
  }
}

/**
 * Wrapper for sessionStorage providing type-safe and convenient behavior
 * */
export class SessionValue<ValueType> extends StorageValue<ValueType> {
  /**
   * Constructor for declaring a new instance of SessionValue
   * @param className The top-level namespace for building the key - recommend using the name of the class using this class
   * @param valueName The name of the value for uniquely identifying it - recommend the name of the variable in the class
   * @param instanceName Unique name of the instance of this SessionValue - Defaults to "static" assuming there is only one value+class combo
   */
  constructor(
    className: string,
    valueName: string,
    instanceName: string = "static"
  ) {
    const key = `${className}::${instanceName}::${valueName}`;
    super(key, sessionStorage);
  }
}
