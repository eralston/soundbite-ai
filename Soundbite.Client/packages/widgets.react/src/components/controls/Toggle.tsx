import React, { ChangeEvent, Component } from "react";

interface IProps {
  onToggle?: (isEnabled: boolean) => void;
  defaultValue?: boolean;
  disabled?: boolean;
  className?: string;
  title?: string;
}

interface IState {
  isEnabled: boolean;
}

/**
 * Renders a boolean toggle with a general iOS-liked look and feel
 * */
export class Toggle extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = {
      isEnabled: props.defaultValue ?? false,
    };
    this.onToggle = this.onToggle.bind(this);
  }

  protected onToggle(event: ChangeEvent<HTMLInputElement>) {
    const isEnabled = event.target.checked;
    this.setState({ isEnabled });
    if (this.props.onToggle) this.props.onToggle(isEnabled);
  }

  render() {
    return (
      <label
        className={`custom-toggle sb-toggle ${this.props.className}`}
        title={this.props.title}
      >
        <input
          type="checkbox"
          defaultChecked={this.state.isEnabled}
          onChange={this.onToggle}
          disabled={this.props.disabled}
          title={
            this.props.title != null ? `Toggle ${this.props.title}` : undefined
          }
        />
        <span
          className="custom-toggle-slider rounded-circle"
          data-label-off="No"
          data-label-on="Yes"
        ></span>
      </label>
    );
  }
}
