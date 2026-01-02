import React, { Component } from "react";

export class Header extends Component {
  render() {
    return (
      <div className="header sb-header bg-gradient-brand pb-4 pt-6 pt-md-8">
        <div className="separator separator-bottom separator-skew zindex-100">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            preserveAspectRatio="none"
            version="1.1"
            viewBox="0 0 2560 100"
            x="0"
            y="0"
          >
            <polygon
              className="fill-default"
              points="2560 0 2560 100 0 100"
            ></polygon>
          </svg>
        </div>
      </div>
    );
  }
}
