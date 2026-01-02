/**
 * Interface that specifies the localized "strings" that are available in the Web part.
 */
declare interface ISoundbiteFeedWebPartStrings {
  groupRouteDisplayName: string;
  titleDisplayName: string;
  hideTitleDisplayName: string;
  feedWebPartPropGroupName: string;
  feedWebPartPropPaneDescription: string;
}

/**
 * Module used to expose the "strings" from the ISoundbiteFeedWebPartStrings interface.
 */
declare module "SoundbiteFeedWebPartStrings" {
  const strings: ISoundbiteFeedWebPartStrings;
  export = strings;
}
