import Axios from "axios";
import moment from "moment";
import {
  CalendarEntry,
  ProviderType,
  RecurrencePattern,
  Recurrence,
} from "@soundbite/api";

import { ICalendarProvider } from "./ICalendarProvider";
import { AadAuthProvider, getAadAuthProvider } from "../auth/AadAuthProvider";

const graphUrl = "https://graph.microsoft.com/v1.0/";

/**
 * Defines the contract required for interacting with a third-party calendar provider.  Each
 * calendar provider must have its own ICalendarProvider implementation.
 */
export class AadCalendarProvider implements ICalendarProvider {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  private _authProvider: AadAuthProvider;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  /**
   * Creates a new CalendarProviderAAD instance.
   */
  constructor() {
    this._authProvider = getAadAuthProvider() as AadAuthProvider;
  }

  //////////[ ICalendarProvider Implementation ]////////////////////////////////////////////////////

  /**
   * Gets the provider type associated with this calendar provider implementation.
   */
  get providerType() {
    return ProviderType.AAD;
  }

  /**
   * Converts numeric day of week values into the string expected by AAD
   * @param daysOfWeek - array containing numeric day of week values
   * @returns a string array with converted values for AAD
   */
  convertDaysOfWeek(daysOfWeek: number[]): string[] {
    let conversion = [
      "sunday",
      "monday",
      "tuesday",
      "wednesday",
      "thursday",
      "friday",
      "saturday",
    ];
    let result: string[] = [];
    daysOfWeek?.forEach((i) => {
      result.push(conversion[i]);
    });
    return result;
  }

  /**
   * Converts the Soundbite recurrency information into a value that can be used in AAD.
   * @param recurrenceType - identifies the type of recurrence
   * @param recurrencePattern - recurrence-specific information for the recurrence type
   */
  convertRecurrencePattern(
    recurrenceType: Recurrence,
    recurrencePattern: RecurrencePattern
  ): any {
    switch (recurrenceType) {
      case Recurrence.NoRepeat:
        return null;
      case Recurrence.Daily:
        return {
          type: "daily",
          interval: 1,
        };
      case Recurrence.Monthly:
        return {
          type: "absoluteMonthly",
          dayOfMonth: recurrencePattern.dayOfMonth,
          interval: 1,
        };
      case Recurrence.Weekday:
        // These both use the same data because the daysOfWeek value differentiates them
        return {
          type: "weekly",
          interval: 1,
          firstDayOfWeek: "sunday",
          daysOfWeek: this.convertDaysOfWeek(recurrencePattern.daysOfWeek),
        };
      case Recurrence.Weekly:
        // These both use the same data because the daysOfWeek value differentiates them
        return {
          type: "weekly",
          interval: 1,
          firstDayOfWeek: "sunday",
          daysOfWeek: this.convertDaysOfWeek(recurrencePattern.daysOfWeek),
        };
      default:
        throw new Error("Unsupported recurrence type");
    }
  }

  /**
   * Responsible for creating a calendar event for the user on the provider.
   */
  async createEvent(data: CalendarEntry): Promise<string> {
    // API REFERENCE: https://docs.microsoft.com/en-us/graph/api/calendar-post-events?view=graph-rest-1.0&tabs=http
    //                https://docs.microsoft.com/en-us/graph/api/user-post-events?view=graph-rest-1.0&tabs=http
    // TIMEZONE REFERENCE: https://docs.microsoft.com/en-us/windows-hardware/manufacture/desktop/default-time-zones
    // Recurrence reference: https://docs.microsoft.com/en-us/graph/api/resources/recurrencepattern?view=graph-rest-1.0

    const url = `${graphUrl}me/calendar/events`;

    const calendarEntry: any = {
      //transactionId: Utils.newGuid(),
      subject: data.subject,
      body: {
        contentType: "HTML",
        content: data.body,
      },
      start: {
        dateTime: moment(data.startTime).toISOString(),
        timeZone: "Greenwich Standard Time",
      },
      end: {
        dateTime: moment(data.startTime)
          .add(data.duration, "minutes")
          .toISOString(),
        timeZone: "Greenwich Standard Time",
      },
      //location: { displayName: "None" }
      //attendees: [],
      //transactionId: "7E163156-7762-4BEB-A1C6-729EA81755A7"
    };

    if (data.recurrence !== Recurrence.NoRepeat) {
      calendarEntry.recurrence = {
        pattern: this.convertRecurrencePattern(
          data.recurrence,
          data.recurrencePattern
        ),
        range: {
          startDate: moment(data.startTime).format("YYYY-MM-DD"),
          type: "noEnd",
        },
      };
    }

    var calendarEntryId = await this._authProvider.wrapGraphCall<string>(
      async (graphToken) => {
        alert(graphToken);
        const call = await Axios.post<any>(url, calendarEntry, {
          headers: { authorization: `Bearer ${graphToken}` },
        });
        alert("succeeded");
        return call.data.id; // Returns the AAD ID of the calendar entry
      }
    );

    return calendarEntryId;
  }
}
