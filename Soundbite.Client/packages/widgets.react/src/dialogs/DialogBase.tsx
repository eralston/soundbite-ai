import React, { Component } from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";

import { DialogAction } from "./DialogAction";
import { DialogStyleType } from "./DialogStyleType";

export abstract class DialogBase {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  title: string = "";
  style: DialogStyleType = DialogStyleType.Normal;
  actions: DialogAction[] = [];

  //////////[ Abstract Members ]////////////////////////////////////////////////////////////////////

  protected abstract bodyContent(): JSX.Element | undefined;

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for rendering the modal dialog header content.  By default the header contains the
   * value defined in the title property.  This can be overridden if desired.
   */
  protected headerContent(): JSX.Element | undefined {
    return <React.Fragment>{this.title}</React.Fragment>;
  }

  /**
   * Responsible for rendering the modal dialog footer content.  By default the footer contains
   * buttons for all items in the actions array property.  This can be overridden if desired.
   */
  protected footerContent(): JSX.Element | undefined {
    return <React.Fragment>FOOTER!</React.Fragment>;
  }

  protected renderHeader(): JSX.Element | undefined {
    const content = this.headerContent();
    if (content) {
      return <ModalHeader>{content}</ModalHeader>;
    } else {
      return undefined;
    }
  }

  protected renderBody(): JSX.Element | undefined {
    const content = this.bodyContent();
    if (content) {
      return <ModalBody>{content}</ModalBody>;
    } else {
      return undefined;
    }
  }

  protected renderFooter(): JSX.Element | undefined {
    const content = this.footerContent();
    if (content) {
      return <ModalFooter>{content}</ModalFooter>;
    } else {
      return undefined;
    }
  }

  Render(): JSX.Element {
    return (
      <Modal>
        {this.renderHeader()}
        <ModalBody>{this.bodyContent()}</ModalBody>
        <ModalHeader>{this.footerContent()}</ModalHeader>
      </Modal>
    );
  }
}
