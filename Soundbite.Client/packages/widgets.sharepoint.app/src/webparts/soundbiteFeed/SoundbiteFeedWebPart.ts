import * as React from "react";
import * as ReactDom from "react-dom";
import { Version } from "@microsoft/sp-core-library";
import {
  IPropertyPaneConfiguration,
  PropertyPaneTextField,
  PropertyPaneCheckbox,
} from "@microsoft/sp-property-pane";
import { BaseClientSideWebPart } from "@microsoft/sp-webpart-base";

import * as strings from "SoundbiteFeedWebPartStrings";
import { SoundbiteFeed } from "./components/SoundbiteFeed";
import { ISoundbiteFeedWebPartProps } from "./interfaces/ISoundbiteFeedWebPartProps";
import { ISoundbiteFeedProps } from "./interfaces/ISoundbiteFeedProps";

/**
 * Feed Web Part
 */
export default class SoundbiteFeedWebPart extends BaseClientSideWebPart<ISoundbiteFeedWebPartProps> {
  public render(): void {
    const element: React.ReactElement<ISoundbiteFeedProps> =
      React.createElement(SoundbiteFeed, {
        part: this,
        context: this.context,
        groupRoute: this.properties.groupRoute,
        title: this.properties.title,
        hideTitle: this.properties.hideTitle,
      });

    ReactDom.render(element, this.domElement);
  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse("1.0");
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.feedWebPartPropPaneDescription,
          },
          groups: [
            {
              groupName: strings.feedWebPartPropGroupName,
              groupFields: [
                PropertyPaneTextField("title", {
                  label: strings.titleDisplayName,
                }),
                PropertyPaneCheckbox("hideTitle", {
                  text: strings.hideTitleDisplayName,
                }),
                PropertyPaneTextField("groupRoute", {
                  label: strings.groupRouteDisplayName,
                }),
              ],
            },
          ],
        },
      ],
    };
  }
}
