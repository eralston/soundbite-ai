import { action, makeObservable, observable } from "mobx";
import { ActivityReport, ContentReport, ReportService } from "@soundbite/api";

/**
 * Orchestrating data for reporting platform and audience activity
 */
class ReportStoreClass {
  //////////[ Observables ]/////////////////////////////////////////////////////////////////////////

  activityReport?: ActivityReport;
  contentReport?: ContentReport;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observables
      activityReport: observable,
      contentReport: observable,
      // Actions
      readActivityReportAsync: action,
      readContentReportAsync: action,
    });
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////
  public async readActivityReportAsync(
    orgRoute: string,
    startDate?: string,
    endUtc?: string
  ): Promise<ActivityReport> {
    this.activityReport = undefined;
    const response = await ReportService.activityReportAsync(
      orgRoute,
      startDate,
      endUtc
    );
    this.activityReport = response;
    return response;
  }

  public async readContentReportAsync(
    orgRoute: string,
    startDate?: string,
    endUtc?: string
  ): Promise<ContentReport> {
    this.contentReport = undefined;
    const response = await ReportService.contentReportAsync(
      orgRoute,
      startDate,
      endUtc
    );
    this.contentReport = response;
    return response;
  }
}

export default new ReportStoreClass();
