import moment from "moment";
import { User, Recurrence, ActivityRangeItem } from "..";
import {
  ParticipantReactionType,
  ParticipantRole,
  ParticipantState,
} from "../generated/enums";
import { FileType } from "../generated/enums";

/** Utility class for general-purpose functions */
export class Utils {
  /**
   * Determines the initial organization route.  This method will return the org route in the URL if
   * present.  If not, the method uses the last org route in local storage. Method will return null
   * when no org route can be determined.
   */
  static getOrgRouteFromPath(path: string | null): string | null {
    let result = null;
    if (path) {
      const orgRegex = new RegExp("/organizations/([^/]*)");
      var orgRegexData = orgRegex.exec(path);
      if (orgRegexData && orgRegexData.length > 1) {
        result = orgRegexData[1];
      }
    }
    return result;
  }

  /**
   * Brute force set the hash for the current page
   * Must be the whole value, EG #sbrecord=12345 not just a component of the hash
   * @param hashValue
   */
  static setHash(hashValue?: string) {
    if (history.pushState) {
      const urlWithNewHash =
        hashValue != null
          ? `${window.location.pathname}#${hashValue}`
          : window.location.pathname;

      history.pushState(null, document.title, urlWithNewHash);
    } else {
      const newHash = hashValue != null ? `#${hashValue}` : "";
      location.hash = newHash;
    }
  }

  static recurrenceDescription(
    dateIsoString: string | undefined,
    recurrence: Recurrence
  ): string {
    if (dateIsoString === undefined) return "";

    const date = moment(dateIsoString).local();

    const monthAndDayNumber = date.format("MMM D");
    const dayOfWeek = date.format("ddd");
    const dayOfMonth = date.date();

    switch (recurrence) {
      case Recurrence.Daily:
        return `Every day starting ${monthAndDayNumber}`;
      case Recurrence.Weekday:
        return `Every Mon-Fri starting ${monthAndDayNumber}`;
      case Recurrence.Weekly:
        return `Every ${dayOfWeek} starting ${monthAndDayNumber}`;
      case Recurrence.Monthly:
        return `Every month on day ${dayOfMonth} starting ${monthAndDayNumber}`;
    }
    return "";
  }

  static secondsToString = (limit = 0) => {
    const minutes = Math.floor(limit / 60);
    const seconds = limit - minutes * 60;

    let ret = ``;
    if (minutes > 0) ret += `${minutes}m`;

    if (minutes > 0 && seconds > 0) ret += ` `;

    if (seconds > 0) ret += `${Math.round(seconds)}s`;

    return ret;
  };

  // NOTE: You should probably use Moment/Luxon to format the time, but this is for quick display
  static displayTime(totalSeconds: number) {
    totalSeconds = Math.round(totalSeconds);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds - minutes * 60;
    const minDisplay = Utils.pad(minutes, 2);
    const secDisplay = Utils.pad(seconds, 2);
    return `${minDisplay}:${secDisplay}`;
  }

  /**
   * For transforming server-side dates to client-side
   * @param date
   */
  static formatDateTime(date: string | undefined) {
    if (date === undefined) return "Unknown";

    return moment.utc(date).local().format("l h:mmA");
  }

  /**
   * Transforms ISO string datetime into date
   * @param date
   */
  static formatDate(date: string | undefined) {
    if (date === undefined) return "Unknown";

    return moment.utc(date).local().format("l");
  }

  static formatRelativeDate(date: string | undefined) {
    if (date === undefined) return "";

    return moment.utc(date).local().calendar();
  }

  static pad(number: number, size: number) {
    let s = String(number);
    while (s.length < (size || 2)) {
      s = "0" + s;
    }
    return s;
  }

  static addMinutes(date: Date, diffInMinutes: number) {
    return new Date(date.getTime() + diffInMinutes * 60000);
  }

  /**
   * Clones the specified object
   * @param obj - object to clone
   */
  static clone<T>(obj: T): T {
    if (obj === null || obj === undefined) {
      return null as unknown as T;
    } else {
      return JSON.parse(JSON.stringify(obj)) as T;
    }
  }

  /**
   * Compares two strings to determine if a is less than, equal to, or greater than b
   * @param a - first string to compare
   * @param b - second string to compare
   * @returns -1 if a should appear before b, 0 if strings are equal, or 1 is a should appear after b.
   */
  static stringCompare(
    a: string | null | undefined,
    b: string | null | undefined
  ) {
    if (a === null || a === undefined) a = "";
    if (b === null || b === undefined) b = "";
    return a.localeCompare(b);
  }

  /**
   * Find the differences between two objects and push to a new object
   * (c) 2019 Chris Ferdinandi & Jascha Brinkmann, MIT License,
   * https://gomakethings.com & https://twitter.com/jaschaio
   * @param obj1 - The original object
   * @param obj2 - The object to compare against it
   * @returns - An object of differences between the two
   */
  static diff(obj1: any, obj2: any) {
    // Make sure an object to compare is provided
    if (!obj2 || Object.prototype.toString.call(obj2) !== "[object Object]") {
      return obj1;
    }

    const diffs: any = Object.create(null);
    let key;

    /**
     * Check if two arrays are equal
     * @param  {Array}   arr1 The first array
     * @param  {Array}   arr2 The second array
     * @return {Boolean}      If true, both arrays are equal
     */
    const arraysMatch = (arr1: any[], arr2: any[]) => {
      // Check if the arrays are the same length
      if (arr1.length !== arr2.length) return false;

      // Check if all items exist and are in the same order
      for (let i = 0; i < arr1.length; i++) {
        if (arr1[i] !== arr2[i]) return false;
      }

      // Otherwise, return true
      return true;
    };

    /**
     * Compare two items and push non-matches to object
     * @param  {*}      item1 The first item
     * @param  {*}      item2 The second item
     * @param  {String} key   The key in our object
     */
    let compare = (item1: any, item2: any, key: string) => {
      // Get the object type
      const type1 = Object.prototype.toString.call(item1);
      const type2 = Object.prototype.toString.call(item2);

      // If type2 is undefined it has been removed
      if (type2 === "[object Undefined]") {
        diffs[key] = null;
        return;
      }

      // If items are different types
      if (type1 !== type2) {
        diffs[key] = item2;
        return;
      }

      // If an object, compare recursively
      if (type1 === "[object Object]") {
        const objDiff = Utils.diff(item1, item2);
        if (Object.keys(objDiff).length > 1) {
          diffs[key] = objDiff;
        }
        return;
      }

      // If an array, compare
      if (type1 === "[object Array]") {
        if (!arraysMatch(item1, item2)) {
          diffs[key] = item2;
        }
        return;
      }

      // Else if it's a function, convert to a string and compare
      // Otherwise, just compare
      if (type1 === "[object Function]") {
        if (item1.toString() !== item2.toString()) {
          diffs[key] = item2;
        }
      } else {
        if (item1 !== item2) {
          diffs[key] = item2;
        }
      }
    };

    // Loop through the first object
    for (key in obj1) {
      if (obj1.hasOwnProperty(key)) {
        compare(obj1[key], obj2[key], key);
      }
    }

    // Loop through the second object and find missing items
    for (key in obj2) {
      if (obj2.hasOwnProperty(key)) {
        if (!obj1[key] && obj1[key] !== obj2[key]) {
          diffs[key] = obj2[key];
        }
      }
    }

    // Return the object of differences
    return diffs;
  }

  /**
   * Generates a cryptographically unsecure GUID useful for temporary applications. This should not
   * be used to set IDs that will end up as keys in databases, etc.
   */
  static newGuid() {
    return "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".replace(/[x]/g, function (c) {
      const r = (Math.random() * 16) | 0,
        v = c === "x" ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  /**
   * Converts the given User to their display text, which is ideally their name falling back to their email
   * @param user User for which display name is generated
   * @param isFirstNameOnly Return only first name if true; otherwise return "First Last"
   */
  public static userDisplay(
    user?: User,
    isFirstNameOnly = false,
    includeEmail = false
  ) {
    if (!user) return "";

    let display = user.email;
    if (user.givenName && isFirstNameOnly) {
      display = `${user.givenName}`.trim();
    } else if (user.givenName || user.familyName) {
      display = `${user.givenName} ${user.familyName}`.trim();
    }

    if (includeEmail && user.email != null) {
      display = `${display} (${user.email})`.trim();
    }

    return display;
  }

  public static validateEmail(email: string): string | undefined {
    if (this.isEmail(email)) return email;
    else return undefined;
  }

  public static isEmail(email: string): boolean {
    email = email.trim().toLowerCase();
    const re =
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
  }

  /**
   * Determines whether the specified string contains a null or empty string value.
   * @param s - string to check for null or empty value.
   * @param trim - (Optional - default true) flag indicating whether to trim a string before checking for empty value.
   */
  public static isNullOrEmpty(
    s: string | null | undefined,
    trim: boolean = true
  ): s is null | undefined {
    if (s !== undefined && s !== null) {
      if (typeof s == "string") {
        // Value is a string so trim it (if requested) and check for empty.
        return (trim ? s.trim() : s) === "";
      }

      // The value is not a string and is not null.
      return false;
    }

    // The value is null
    return true;
  }

  /**
   * Determines the FileType from a FileName
   * @param fileName - name of the file
   * @returns a FileType value based on the fileName value
   */
  public static GetFileTypeFromFileName(fileName: string): FileType {
    const parts: string[] = fileName.split(".");
    const ext: string = parts[parts.length - 1].toLowerCase();
    switch (ext) {
      case "avi":
        return FileType.Avi;
      case "f4v":
        return FileType.F4v;
      case "flv":
        return FileType.Flv;
      case "mkv":
        return FileType.Mkv;
      case "mov":
        return FileType.Mov;
      case "mp3":
        return FileType.Mp3;
      case "mp4":
        return FileType.Mp4;
      case "mpg":
        return FileType.Mpg;
      case "webm":
        return FileType.Webm;
      case "wmv":
        return FileType.Wmv;
      default:
        return FileType.Unknown;
    }
  }

  /**
   * Determines whether the specified FileType represents video
   * @param fileType FileType value
   * @returns true if the FileType is a video file type
   */
  public static IsVideo(fileType: FileType): boolean {
    switch (fileType) {
      case FileType.Mp4:
      case FileType.Mpg:
      case FileType.Webm:
      case FileType.Mov:
      case FileType.Mkv:
      case FileType.Wmv:
      case FileType.Avi:
      case FileType.Flv:
      case FileType.F4v:
        return true;
      default:
        return false;
    }
  }

  /**
   * Determines whether the specified FileType represents audio
   * @param fileType FileType value
   * @returns true if the FileType is an audio file type
   */
  public static IsAudio(fileType: FileType): boolean {
    return !Utils.IsVideo(fileType);
  }

  public static stringToHashPercent(name: string) {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = (hash * 31 + name.charCodeAt(i)) % Number.MAX_SAFE_INTEGER;
    }
    return (hash / Number.MAX_SAFE_INTEGER) * 100;
  }

  public static participantRoleString(role: ParticipantRole): string {
    switch (role) {
      case ParticipantRole.Host:
        return "Host";
      case ParticipantRole.Participant:
        return "Participant";
      case ParticipantRole.Audience:
        return "Audience";
      default:
        return "Unknown";
    }
  }

  public static participantStateString(state: ParticipantState): string {
    switch (state) {
      case ParticipantState.Pending:
        return "Pending";
      case ParticipantState.ContributionRequested:
        return "Contribution Requested";
      case ParticipantState.Contributed:
        return "Contributed";
      case ParticipantState.ConsumptionRequested:
        return "Consumption Requested";
      case ParticipantState.Consumed:
        return "Consumed";
      case ParticipantState.Unsubscribed:
        return "Unsubscribed";
      default:
        return "Unknown";
    }
  }

  public static participantReactionString(
    reaction: ParticipantReactionType
  ): string {
    switch (reaction) {
      case ParticipantReactionType.None:
        return "None";
      case ParticipantReactionType.Like:
        return "Like";
      case ParticipantReactionType.Love:
        return "Love";
      case ParticipantReactionType.Laugh:
        return "Laugh";
      case ParticipantReactionType.Wow:
        return "Wow";
      case ParticipantReactionType.Sad:
        return "Frown";
      default:
        return "Unknown";
    }
  }

  public static fillMissingDates(
    items: ActivityRangeItem[]
  ): ActivityRangeItem[] {
    // If there's no data, or only one data point, no need to process further
    if (items.length <= 1) return [];

    const filledItems: ActivityRangeItem[] = [];
    let currentMoment = moment.utc(items[0].label);

    for (let i = 0; i < items.length - 1; i++) {
      const currentLabel = items[i].label;
      const nextLabel = items[i + 1].label;

      // Push current item
      filledItems.push(items[i]);

      // Check for date difference and fill missing dates
      while (currentMoment.add(1, "days").isBefore(moment.utc(nextLabel))) {
        filledItems.push({
          label: currentMoment.utc().format(),
          value: 0,
        });
      }
    }

    // Push the last item
    filledItems.push(items[items.length - 1]);

    return filledItems;
  }
}
