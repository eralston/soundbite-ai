import { SoundbiteApiConfig } from "@soundbite/api";
import React, { Component } from "react";

import { ShowWhen } from "./controls";

interface IProps {
  prompt: string;
  hasBorder?: boolean;
  showImage?: boolean;
}

/** Formatting for an empty card which prominently displays a graphic and some text */
export class EmptyCardBody extends Component<IProps> {
  render() {
    const showImage: boolean = this.props.showImage === false ? false : true;
    let wrapperClassName = "card-header border-0 sb-empty-card-body";
    if (this.props.hasBorder) wrapperClassName += " sb-empty-card-with-border";

    return (
      <div className={wrapperClassName}>
        <div className="align-items-center">
          <div className="text-muted text-center">
            {this.props.prompt}
            <div className="mt-3">{this.props.children}</div>
          </div>
          <div>
            <ShowWhen is={showImage}>
              <img
                src={SoundbiteApiConfig.imgEmptyCardArtUrl}
                alt="Empty Art"
                className="mx-auto d-block"
              />
            </ShowWhen>
          </div>
        </div>
      </div>
    );
  }
}
