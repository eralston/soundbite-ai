import React from 'react';
import ReactDOM from 'react-dom';
import {
  IPublicPlayerWidgetProps, PublicPlayerWidget, SbStyles,
  SbBootstrapStyles,
  SbDashboardStyles
  } from '@soundbite/widgets-react';

/**
 * Exposes a series of methods for injecting Soundbite Widgets into HTML elements within a browser.
 */
class WidgetInjectorClass {

  /**
   * Responsibe for tracking whether the style components have been rendered into the browser.
   */
  private hasRenderedStyles: boolean = false;

  /**
   * Responsible for ensuring that the style components that are required for the look and feel of
   * the widgets have been rendered into the browser.
   */
  private EnsureStylesRendered(): void {
    if (!this.hasRenderedStyles) {
      const tag = document.createElement("div");
      tag.style.display = "none";
      ReactDOM.render((<>
        <SbBootstrapStyles />
        <SbDashboardStyles />
        <SbStyles />
      </>), tag);
      this.hasRenderedStyles = true;
    }
  }

  /**
   * Renders the PublicPlayerWidget into the specified HTML element.
   * @querySelector - specifies the query used to obtain the element into which the widget is injected.
   * @settings - specifies the settings to pass into the widget.
   */
  PublicPlayerWidget(querySelector: string, settings: IPublicPlayerWidgetProps): void {
    this.EnsureStylesRendered();
    ReactDOM.render((<PublicPlayerWidget {...settings} />), document.querySelector(querySelector));
  }

}

export const WidgetInjector = new WidgetInjectorClass();
