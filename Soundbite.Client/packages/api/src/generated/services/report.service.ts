/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { ActivityReport, OrgContentReport, SessionContentDetailsReport, SessionContentReport } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.ReportController
 **/
class ReportServiceClass {

  /** 
  * Automatically generated API call
  */
  async activityReportAsync(orgRoute: string, startUtc?: string, endUtc?: string, options?: IHttpRequestOptions
): Promise<ActivityReport> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/reports/activity?startUtc=${encodeURIComponent(startUtc ?? "")}&endUtc=${encodeURIComponent(endUtc ?? "")}`;
    const response = await HttpService.post<ActivityReport>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async contentReportAsync(orgRoute: string, startUtc?: string, endUtc?: string, options?: IHttpRequestOptions
): Promise<OrgContentReport> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/reports/content?startUtc=${encodeURIComponent(startUtc ?? "")}&endUtc=${encodeURIComponent(endUtc ?? "")}`;
    const response = await HttpService.post<OrgContentReport>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async sessionReportAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<SessionContentReport> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sessions/${encodeURIComponent(sessionRoute)}/reports/content`;
    const response = await HttpService.post<SessionContentReport>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async sessionDetailsReportAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<SessionContentDetailsReport> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sessions/${encodeURIComponent(sessionRoute)}/reports/content/details`;
    const response = await HttpService.post<SessionContentDetailsReport>(url, null, options);
    return response;
  }
}

export const ReportService = new ReportServiceClass();
