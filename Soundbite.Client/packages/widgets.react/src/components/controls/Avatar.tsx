import React, { Component } from "react";
import { SoundbiteApiConfig, User, Utils } from "@soundbite/api";
import { ShowWhen } from "..";

interface IProps {
  user: User;
  alwaysHasEmail?: boolean;
  onClick?: () => void;
  className?: string;
  title?: string;
  showJobTitle?: boolean;
  showName?: boolean;
  sizingClassName?: string;
}

/** Displays the given user in a standardized format, scaling by viewport size */
export class Avatar extends Component<IProps> {
  protected onClick = () => {
    if (this.props.onClick) this.props.onClick();
  };

  render() {
    const showName = this.props.showName ?? true;
    const showTitle =
      this.props.showJobTitle === true && this.props.user.title != null;
    let avatarlUrl = SoundbiteApiConfig.imgAvatarUrl;

    if (this.props.user.imageSrc) avatarlUrl = this.props.user.imageSrc;

    let displayName = Utils.userDisplay(this.props.user);

    let outerClass = "media align-items-center";
    if (this.props.onClick) outerClass += " sb-clickable";

    if (this.props.className) outerClass += " " + this.props.className;

    const paddingRight = showName || showTitle ? "mr-3" : "mr-1";
    const sizingClassName = this.props.sizingClassName ?? "d-none d-sm-block";

    return (
      <div
        className={outerClass}
        onClick={this.onClick.bind(this)}
        title={this.props.title}
      >
        <span
          className={`avatar rounded-circle ${paddingRight} ${sizingClassName}`}
        >
          <img alt="Avatar" src={avatarlUrl} />
        </span>
        <div className="media-body">
          <ShowWhen is={this.props.alwaysHasEmail && showName}>
            <span className="name mb-0 text-sm">{displayName}</span>
          </ShowWhen>
          <ShowWhen is={!this.props.alwaysHasEmail && showName}>
            <div className="d-md-none">
              <span className="name mb-0 text-sm">{displayName}</span>
            </div>
            <div className="d-none d-md-block">
              <span className="name mb-0 text-sm">{displayName}</span>
            </div>
          </ShowWhen>
          <ShowWhen is={showTitle}>
            <div>
              <span className="name mb-0 text-sm text-muted font-weight-light">
                {this.props.user.title}
              </span>
            </div>
          </ShowWhen>
        </div>
      </div>
    );
  }
}
