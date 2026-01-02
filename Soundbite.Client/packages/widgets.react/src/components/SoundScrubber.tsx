/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { Component } from "react";
import { Scrubber } from "react-scrubber";

import { ClipWithContributor, Utils } from "@soundbite/api";

import { ErrorDlg } from "./ErrorDlg";
import { GlobalTheme } from "../styles";
import moment from "moment";
import { Loader } from "./controls";

function getStyles() {
  const styles = css`
    width: 100%;
    height: 1.5rem;
    padding-left: 0.75rem;
    padding-right: 0.75rem;
    background-color: ${GlobalTheme.current.colors.neutrals.n500};
    border-radius: 4px;
    overflow: hidden;
    position: relative;

    .sb-time-summary {
      color: ${GlobalTheme.current.colors.neutrals.n200};
      right: 0.5rem;
      pointer-events: none;
      position: absolute;
      top: 1px;
      z-index: 1;
    }

    &[aria-disabled="true"] {
      pointer-events: none;
    }

    &[aria-disabled="true"] .sb-time-summary {
      color: ${GlobalTheme.current.colors.neutrals.n300};
    }

    .scrubber.horizontal .bar {
      height: 100%;
    }

    .scrubber.horizontal .bar .bar__thumb {
      height: 1.5rem;
      width: 1.5rem;
      transform: translate(-50%, -50%);
      top: 50%;
    }

    .scrubber {
      width: 100%;
      height: 100%;
      position: relative;
      -webkit-user-select: none;
      user-select: none;
      touch-action: none;
    }

    .scrubber .bar {
      background-color: transparent;
      width: 100;
      position: relative;
      transition: height 0.2s linear, width 0.2s linear;
    }

    .scrubber * {
      -webkit-user-select: none;
      user-select: none;
    }

    .scrubber .bar__progress {
      position: absolute;
      background-color: ${GlobalTheme.current.colors.neutrals.n600};
      border-top-left-radius: 4px;
      border-bottom-left-radius: 4px;
      left: -0.75rem;
    }

    .scrubber .bar__buffer {
      position: absolute;
    }

    .scrubber .bar__thumb {
      position: absolute;
      width: 0px;
      height: 0px;
      border-radius: 4px;
      transition: height 0.2s linear, width 0.2s linear;
      background-color: ${GlobalTheme.current.colors.bootstrap.primary};
    }

    &[aria-disabled="true"] .scrubber .bar__thumb {
      background-color: ${GlobalTheme.current.colors.neutrals.n700};
    }

    .scrubber .bar__marker {
      position: absolute;
    }

    .scrubber.horizontal .bar__progress,
    .scrubber.horizontal .bar__marker,
    .scrubber.horizontal .bar__buffer {
      height: 100%;
    }

    .scrubber.horizontal .bar__progress::after {
      position: absolute;
      background-color: ${GlobalTheme.current.colors.neutrals.n600};
      right: -4px;
      width: 5px;
      height: 100%;
      content: " ";
    }

    .scrubber.horizontal .bar__marker {
      width: 12px;
    }

    .scrubber.horizontal .bar {
      top: 50%;
      left: 0;
      transform: translateY(-50%);
      width: 100%;
    }

    .scrubber.vertical .bar__progress,
    .scrubber.vertical .bar__marker,
    .scrubber.vertical .bar__buffer {
      width: 100%;
      bottom: 0;
    }

    .scrubber.vertical .bar {
      top: 0;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      height: 100%;
    }

    .scrubber.vertical .bar__thumb {
      transform: translate(-50%, 50%);
      left: 50%;
    }

    .scrubber.hover.vertical .bar {
      width: 6px;
    }

    .scrubber.vertical .bar__marker {
      height: 100%;
    }
  `;

  return styles;
}

interface IProps {
  clip?: ClipWithContributor;
  initialMediaUrl?: string;
  onTick?: (Scrubber: SoundScrubber, timer: number) => void;
  onLoaded?: (scrubber: SoundScrubber) => void;
  onScrub?: (scrubber: SoundScrubber) => void;
  onEnded?: (scrubber: SoundScrubber) => void;
  disabled?: boolean;
  hideTime?: boolean;
  onDuration?: (duration?: number) => void;
  onAudioError?: (ex: any) => void;
}

interface IState {
  duration: number;
  time: number;
  resetOnPlay: boolean;
}

/** Manages audio playback, present a "scrubbable" timeline view w/ clip time */
export class SoundScrubber extends Component<IProps, IState> {
  audio?: HTMLAudioElement;
  timer?: any;

  constructor(props: IProps) {
    super(props);

    this.state = { duration: 0, time: 0, resetOnPlay: false };

    if (this.props.clip) {
      this.doLoadAudioAsync(this.props.clip.url).catch((ex) => {
        if (this.props.onAudioError) {
          this.props.onAudioError(ex);
        }
      });
    } else if (this.props.initialMediaUrl) {
      this.doLoadAudioAsync(this.props.initialMediaUrl).catch((ex) => {
        if (this.props.onAudioError) {
          this.props.onAudioError(ex);
        }
      });
    }
  }

  componentWillUnmount() {
    if (this.timer) clearInterval(this.timer);

    if (this.audio) this.audio.pause();
  }

  protected showFileError(err: Error) {
    ErrorDlg.show(
      err,
      "Unable to Load Audio",
      "Unable to load file; potentially due to incorrect file type. Please contact the host and ensure they are using the Soundbite audio recorder or uploading valid MP3 files."
    );
  }

  private doLoadAudioAsync(mediaUrl: string): Promise<void> {
    this.audio = new Audio(mediaUrl);
    this.audio.onended = this.onAudioEnded.bind(this);
    this.audio.preload = "none";
    this.audio.autoplay = false;
    this.audio.load();
    this.audio.playbackRate = 1.0;

    if (this.props.onDuration != null) {
      this.props.onDuration(undefined);
    }

    return new Promise((resolve, reject) => {
      if (this.audio) {
        this.audio.onerror = () => {
          if (this.audio && this.audio.error) {
            const err = new Error(this.audio.error.message);
            this.showFileError(err);
          }
          reject();
        };
        this.audio.oncanplaythrough = () => {
          if (this.audio) {
            this.setState({
              duration: this.props.clip?.seconds ?? this.audio.duration,
            });
            if (this.props.onDuration != null) {
              if (this.props.clip) {
                this.props.onDuration(this.props.clip.seconds);
              } else {
                this.props.onDuration(this.audio.duration);
              }
            }
            resolve();
          } else {
            const err = new Error("Empty audio in oncanplaythrough");
            this.showFileError(err);
            reject();
          }
        };
      } else {
        const err = new Error(
          "Empty audio after trying to load in doLoadAudioAsync"
        );
        this.showFileError(err);
        reject();
      }
    });
  }

  private onAudioEnded(this: SoundScrubber) {
    this.end();

    if (this.props.onEnded) this.props.onEnded(this);
  }

  private tick() {
    this.setState({ time: this.state.time + 1 });
    if (this.props.onTick) this.props.onTick(this, this.state.time);
  }

  private onScrub(time: number) {
    this.jump(time);

    if (this.props.onScrub) this.props.onScrub(this);
  }

  get duration(): number {
    if (this.props.clip) return this.props.clip.seconds;
    if (this.audio) return this.audio.duration;
    else return 0;
  }

  async loadUrlAsync(mediaUrl: string): Promise<void> {
    this.setState({ duration: 0, time: 0 });
    await this.doLoadAudioAsync(mediaUrl);
  }

  play() {
    if (!this.timer) {
      this.timer = setInterval(this.tick.bind(this), 1000);
      if (this.state.resetOnPlay) {
        this.setState({ resetOnPlay: false, time: 0 });
      }
      if (this.audio) this.audio.play();
      else console.warn("Trying to play an empty audio");
    }
  }

  pause() {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = undefined;
    }

    if (this.audio) this.audio.pause();
    else console.warn("Trying to pause an empty audio");
  }

  jump(time: number) {
    if (this.audio) this.audio.currentTime = time;
    else console.warn("Trying to jump an empty audio");

    this.setState({ time: time });
  }

  reset() {
    this.jump(0);
    this.pause();
  }

  end() {
    this.setState({ time: this.state.duration, resetOnPlay: true });
    this.pause();
  }

  clear() {
    this.setState({ time: 0, duration: 0 });
    this.audio?.pause();
    this.audio = undefined;
  }

  displayDuration(
    display: moment.Duration,
    totalDuration: moment.Duration
  ): string {
    if (totalDuration.hours() > 0) {
      return `${Utils.pad(display.hours(), 2)}:${Utils.pad(
        display.minutes(),
        2
      )}:${Utils.pad(display.seconds(), 2)}`;
    } else {
      const minPad = totalDuration.minutes() > 9 ? 2 : 1;
      return `${Utils.pad(display.minutes(), minPad)}:${Utils.pad(
        display.seconds(),
        2
      )}`;
    }
  }

  timeSummary() {
    if (this.props.hideTime) {
      return <React.Fragment />;
    }

    const time = moment.duration(this.state.time * 1000);
    const duration = moment.duration(this.state.duration * 1000);

    let timeDisplay = this.displayDuration(time, duration);
    let durationDisplay = this.displayDuration(duration, duration);

    return (
      <span className="sb-time-summary">
        {timeDisplay}/{durationDisplay}
      </span>
    );
  }

  render() {
    const styles = getStyles();
    const scrubberClassName =
      "sb-sound-scrubber" +
      (this.props.disabled ? " sb-sound-scrubber-disabled" : "");
    return (
      <Loader isLoadedWhen={this.state.duration > 0} isSmall={true}>
        <div
          css={styles}
          className={scrubberClassName}
          aria-disabled={this.props.disabled}
        >
          {this.timeSummary()}
          <Scrubber
            min={0}
            max={this.state.duration}
            value={this.state.time}
            onScrubStart={this.onScrub.bind(this)}
            onScrubEnd={this.onScrub.bind(this)}
            onScrubChange={this.onScrub.bind(this)}
          />
        </div>
      </Loader>
    );
  }
}
