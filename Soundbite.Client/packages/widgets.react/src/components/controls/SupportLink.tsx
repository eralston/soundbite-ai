import React, { Component } from "react";
import { PublicError } from "@soundbite/api";

interface IProps {
  error?: PublicError;
}

interface IState {
  link: string;
}

/**
 * React component for managing a singleton error dialog
 * If the error is an instance of PublicError, it will show it to the user in the dialog
 * Otherwise, it will console.error with a helpful header on the message
 * */
export class SupportLink extends Component<IProps, IState> {
  get mailtoLink() {
    // Build a smart mailto link capable of kicking off a support request via email
    const encodedTo = encodeURIComponent(`Support@Soundbite.Freshdesk.Com`);
    const encodedSubject = encodeURIComponent(
      `Soundbite Error Support Request: ${this.props.error?.name || "Error"}`
    );
    const errName = this.props.error?.name || "Error";
    const errMsg = this.props.error?.message || "No Message";

    const currentUrl = window.location.href;

    const inErrName = this.props.error?.innerError?.name || "No Inner Error";
    const inErrMsg =
      this.props.error?.innerError?.message || "No Inner Error Message";
    const inErrStack = this.props.error?.innerError?.stack || "No Stack";

    // TODO: Include current user and organization information
    const encodedBody = encodeURIComponent(
      `Support Request: ${errName}\n${errMsg}\n\nOn page: ${currentUrl}\n\nInner Error:\n${inErrName}\n${inErrMsg}\n${inErrStack}`
    );

    const mailtoLink = `mailto:${encodedTo}?subject=${encodedSubject}&body=${encodedBody}`;
    return mailtoLink;
  }

  render() {
    return <a href={this.mailtoLink}>{this.props.children}</a>;
  }
}
