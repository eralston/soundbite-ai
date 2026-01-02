import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faRedo } from "@fortawesome/free-solid-svg-icons";
import classnames from "classnames";
import React, { Component, createRef } from "react";
import {
  Col,
  Media,
  Nav,
  NavItem,
  NavLink,
  Row,
  TabContent,
  TabPane,
} from "reactstrap";

import { ClipWithContributor, UserRole, Utils } from "@soundbite/api";

import { GlobalTheme } from "../styles";
import { OrganizationStore } from "../store/OrganizationStore";
import { DialogState, ClipSource } from "../enums";
import { SoundScrubber } from "./SoundScrubber";
import { Player } from "./Player";
import { Loader, SbProgressButton, ShowWhen } from "./controls";
import { DownloadBtn } from "./controls/DownloadBtn";
import { VideoEditor } from "./video/VideoEditor";
import { VideoClip } from "./video/VideoClip";
import { WidgetStore } from "../store";

var rmg: any = require("../components/react-mic-gold");

interface IProps {
  limitInSeconds?: number;
  waveColor?: string;
  waveBackgroundColor?: string;
  disabled?: boolean;

  /** Called when the recorder changes in its availability life-cycle */
  onChange?: (state: DialogState) => void;
  onClipSourceChange?: (source: ClipSource) => void;
}

interface IState {
  dlgState: DialogState;
  time: number;
  recordedDuration: number;
  mediaUrl?: string;
  isPlayMode: boolean;
  timer?: any;
  uploadFile?: File;
  uploadDuration?: number;
  audioFile?: File;
  videoFile?: VideoClip;
  activeTabValue: ClipSource;
  audioError: boolean;
}

/** A multi-purpose component that can both record in-browser and accept uploads via files */
export class RecordAndPlay extends Component<IProps, IState> {
  private scrubber = createRef<SoundScrubber>();
  private sampleRate?: number;

  constructor(props: IProps) {
    super(props);

    const isVideoEnabled =
      WidgetStore.organizations.currentOrg?.settings.sessions.videoEnabled ===
      true;

    const defaultTab = isVideoEnabled
      ? ClipSource.RecordVideo
      : ClipSource.RecordAudio;
    this.state = {
      time: 0,
      recordedDuration: 0,
      audioFile: undefined,
      dlgState: DialogState.Empty,
      isPlayMode: false,
      activeTabValue: defaultTab,
      uploadFile: undefined,
      uploadDuration: 0,
      audioError: false,
    };

    if (this.props.onClipSourceChange) {
      this.props.onClipSourceChange(this.state.activeTabValue);
    }
    this.onAudioRecorded = this.onAudioRecorded.bind(this);
    this.onVideoRecorded = this.onVideoRecorded.bind(this);
    this.tick = this.tick.bind(this);
    this.onResetAudio = this.onResetAudio.bind(this);
    this.onResetVideo = this.onResetVideo.bind(this);
    this.onScrub = this.onScrub.bind(this);
    this.onScrubberEnded = this.onScrubberEnded.bind(this);
    this.onClick = this.onClick.bind(this);
  }

  componentWillUnmount() {
    this.clearTimer();
  }

  get isRunning() {
    return !!this.state.timer;
  }

  private tick() {
    // We enforce recording limits, but rely on the Ended event from the scrubber for play limits
    if (!this.state.isPlayMode) {
      if (
        this.state.time >=
        (this.props.limitInSeconds || Number.MAX_SAFE_INTEGER)
      ) {
        this.stop();
        return;
      }
    }

    this.setState({ time: this.state.time + 1 });
  }

  public get file(): File | undefined {
    switch (this.state.activeTabValue) {
      case ClipSource.RecordAudio:
        return this.state.audioFile;
      case ClipSource.RecordVideo:
        return this.state.videoFile?.file;
      default:
        return this.state.uploadFile;
    }
  }

  public get duration(): number | undefined {
    if (this.state.activeTabValue === ClipSource.RecordAudio) {
      return this.state.recordedDuration;
    } else {
      return this.state.uploadDuration;
    }
  }

  private startTimer() {
    if (!this.state.timer) {
      this.setState({ timer: setInterval(this.tick, 1000) });
    }
  }

  private clearTimer() {
    if (this.state.timer) {
      clearInterval(this.state.timer);
      this.setState({ timer: undefined });
    }
  }

  async playAsync() {
    if (this.state.time >= this.state.recordedDuration) {
      this.setState({ time: 0 });
      this.scrubber.current?.reset();
    }

    this.scrubber.current?.play();

    this.startTimer();
  }

  start() {
    if (this.state.isPlayMode) {
      this.playAsync();
    } else {
      this.setState({ time: 0, recordedDuration: 0 });
      this.startTimer();
    }
  }

  stop() {
    this.clearTimer();

    if (this.state.isPlayMode) {
      this.scrubber.current?.pause();
    } else {
      this.setState({ isPlayMode: true });
    }
  }

  private set dlgState(state: DialogState) {
    this.setState({ dlgState: state });
    this.props.onChange && this.props.onChange(state);
  }

  private async loadAudioAsync(audioUrl: string) {
    this.dlgState = DialogState.Waiting;

    if (this.scrubber.current) {
      await this.scrubber.current.loadUrlAsync(audioUrl);
      this.setState({
        recordedDuration: this.scrubber.current?.duration,
        time: 0,
      });
    }

    this.dlgState = DialogState.Ready;
  }

  private onResetAudio() {
    this.setState({
      recordedDuration: 0,
      time: 0,
      isPlayMode: false,
      audioFile: undefined,
      mediaUrl: undefined,
    });

    this.scrubber.current?.clear();

    this.dlgState = DialogState.Empty;
  }

  private async onScrubberEnded() {
    this.setState({ time: this.state.recordedDuration });
    this.clearTimer();
  }

  private onScrub(scrubber: SoundScrubber) {
    this.setState({ time: scrubber.state.time });
  }

  async onAudioRecorded(recordedBlob: any) {
    const file = new File([recordedBlob.blob], "clip.mp3");
    this.setState({ audioFile: file, mediaUrl: recordedBlob.blobURL });
    this.loadAudioAsync(recordedBlob.blobURL);
  }

  async onVideoRecorded(clip: VideoClip) {
    this.setState({ videoFile: clip, mediaUrl: clip.url });
    this.dlgState = DialogState.Ready;
  }

  async onFileUploaded(uploadFile: File | undefined) {
    this.setState({ uploadFile, audioError: false });
    this.dlgState = DialogState.Ready;
  }

  async onResetVideo() {
    this.setState({ videoFile: undefined, mediaUrl: undefined });
    this.dlgState = DialogState.Empty;
  }

  private onClick() {
    if (this.sampleRate == null) {
      const audioCtx = new (window.AudioContext ||
        (window as any).webkitAudioContext)();
      audioCtx.resume();
      this.sampleRate = audioCtx?.sampleRate;
    }
    if (this.isRunning) this.stop();
    else this.start();
  }

  // RENDERING

  timerText() {
    let left = this.state.time;
    const limitInSeconds = this.props.limitInSeconds;
    if (limitInSeconds === undefined) {
      return (
        <div className="sb-timer text-muted">{Utils.displayTime(left)}</div>
      );
    } else {
      let right = this.state.isPlayMode
        ? this.state.recordedDuration
        : limitInSeconds;
      if (left > right) left = right;

      return (
        <div className="sb-timer text-muted">
          {Utils.displayTime(left)} / {Utils.displayTime(right)}
        </div>
      );
    }
  }

  hasClip(tabValue: ClipSource) {
    switch (tabValue) {
      case ClipSource.RecordAudio:
        return this.state.audioFile !== undefined;
      case ClipSource.RecordVideo:
        return this.state.videoFile !== undefined;
      default:
        return this.state.uploadFile !== undefined;
    }
  }

  toggleTab(tabValue: ClipSource) {
    if (this.state.activeTabValue !== tabValue) {
      this.setState({ activeTabValue: tabValue });
      this.dlgState = this.hasClip(tabValue)
        ? DialogState.Ready
        : DialogState.Empty;
      if (this.props.onClipSourceChange) {
        this.props.onClipSourceChange(tabValue);
      }
    }
  }

  createTab(title: string, value: ClipSource) {
    const className = classnames(
      { active: this.state.activeTabValue === value },
      "sb-tab-btn"
    );

    return (
      <NavItem>
        <NavLink
          role="tab"
          aria-selected={this.state.activeTabValue === value}
          className={className}
          onClick={() => {
            this.toggleTab(value);
          }}
          disabled={this.props.disabled}
          title={title}
        >
          {title}
        </NavLink>
      </NavItem>
    );
  }

  recordAndPlayTab() {
    const waveColor =
      this.props.waveColor ?? GlobalTheme.current.colors.bootstrap.primary;
    const waveBackColor =
      this.props.waveBackgroundColor ??
      GlobalTheme.current.colors.neutrals.midground;

    const rowClass =
      "sb-recorder " + (this.isRunning ? "sb-recorder-running" : "");
    const recordTitle = this.isRunning
      ? "Stop"
      : this.state.isPlayMode
      ? "Play"
      : "Record";

    return (
      <Row className={rowClass}>
        <Col>
          {/*{this.timerText()}*/}
          <ShowWhen
            is={
              this.state.isPlayMode &&
              !this.isRunning &&
              OrganizationStore.currentOrg?.details.me?.user.userRole ===
                UserRole.God &&
              !this.props.disabled &&
              this.state.mediaUrl !== undefined
            }
          >
            <DownloadBtn
              mediaUrl={this.state.mediaUrl}
              outline={true}
              filename="Clip.mpg"
            />
          </ShowWhen>
          <div className="sb-record-button">
            <SbProgressButton
              onClick={this.onClick}
              isRunning={this.isRunning}
              allowClickOnLoading={true}
              disabled={this.props.disabled}
              isRecordMode={!this.state.isPlayMode}
              title={recordTitle}
            />
          </div>
          <ShowWhen is={this.state.isPlayMode && !this.isRunning}>
            <button
              className="btn btn-outline-primary btn-sm sb-record-reset-btn"
              type="button"
              onClick={this.onResetAudio}
              disabled={this.props.disabled}
              title="Reset Recording"
            >
              <span className="btn-inner--icon">
                <FontAwesomeIcon icon={faRedo} />
              </span>
            </button>
          </ShowWhen>
          <ShowWhen is={!this.state.isPlayMode && this.isRunning}>
            {this.timerText()}
            <rmg.ReactMicGold
              record={
                !this.state.isPlayMode && this.isRunning && !this.props.disabled
              }
              visualSetting="frequencyBars"
              className="sb-react-mic"
              strokeColor={waveColor}
              backgroundColor={waveBackColor}
              channelCount={1}
              onStop={this.onAudioRecorded}
              noiseSuppression={true}
              echoCancellation={true}
              sampleRate={this.sampleRate}
            />
          </ShowWhen>
          <ShowWhen is={this.state.isPlayMode}>
            <SoundScrubber
              ref={this.scrubber}
              onScrub={this.onScrub}
              onEnded={this.onScrubberEnded}
              disabled={this.props.disabled}
              onAudioError={(ex) => this.setState({ audioError: true })}
            />
          </ShowWhen>
        </Col>
      </Row>
    );
  }

  uploadTab() {
    const mediaUrl =
      this.state.uploadFile != null
        ? URL.createObjectURL(this.state.uploadFile)
        : undefined;

    const fileDisplayName = this.state.audioError
      ? "Invalid Audio File"
      : this.state.uploadFile != null
      ? this.state.uploadFile.name
      : "No File Chosen";

    const fileType = Utils.GetFileTypeFromFileName(
      this.state.uploadFile?.name ?? ""
    );

    return (
      <Row>
        <Col className="text-center">
          <button
            className="btn btn-outline-secondary"
            onClick={() => {
              document.getElementById("sbUploadBtn")?.click();
            }}
            title="Select File"
          >
            Select File
          </button>
          <span> {fileDisplayName}</span>
          <input
            id="sbUploadBtn"
            type="File"
            accept=".avi,.f4v,.flv,.mkv,.mov,.mp3,.mp4,.mpg,.mp3,.wav,.webm,.wmv"
            style={{ display: "none" }}
            color="secondary"
            disabled={this.props.disabled}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
              const uploadFile = e.target.files?.[0] ?? undefined;
              this.onFileUploaded(uploadFile);
            }}
          />
          <ShowWhen
            is={this.state.uploadFile != null && !this.state.audioError}
          >
            <ShowWhen is={Utils.IsVideo(fileType)}>
              <div className="pb-4">
                {/******************
                 * NOTE: Opted for a simple video element here instead of a player + video element
                 * reference because the RecordAndPlay is using the "old" component style of syntax
                 * and I was having issues using React useRef hook to establish a connection between
                 * the video element and the player.  When/If RecordAndPlay is updated to be a
                 * function component then we should change this to use player + video element
                 * reference.
                 *******************/}
                <video
                  playsInline={true}
                  controls
                  src={mediaUrl}
                  style={{
                    marginTop: "10px",
                    marginBottom: "10px",
                    objectFit: "contain",
                  }}
                  className="sb-video"
                />
              </div>
            </ShowWhen>
            <ShowWhen is={Utils.IsAudio(fileType)}>
              <div className="pb-4 d-flex">
                <Player
                  clip={
                    { url: mediaUrl, fileType: fileType } as ClipWithContributor
                  }
                  onDuration={(duration?: number) =>
                    this.setState({ uploadDuration: duration })
                  }
                  onMediaError={(ex) => this.setState({ audioError: true })}
                  isPublic={false}
                />
              </div>
            </ShowWhen>
          </ShowWhen>
        </Col>
      </Row>
    );
  }

  render() {
    const isVideoEnabled =
      WidgetStore.organizations.currentOrg?.settings.sessions.videoEnabled ===
      true;
    return (
      <div
        className="sb-record-and-play mb-2"
        aria-disabled={this.props.disabled}
      >
        <Loader
          isLoadedWhen={this.state.dlgState !== DialogState.Waiting}
          isDisplayBased={true}
          style={{ minHeight: "122px" }}
        >
          <Nav className="sb-2-tab nav-fill flex-row" role="tablist">
            {isVideoEnabled
              ? this.createTab("Video", ClipSource.RecordVideo)
              : null}
            {this.createTab("Audio", ClipSource.RecordAudio)}
            {this.createTab("Upload", ClipSource.Upload)}
          </Nav>
          <TabContent activeTab={this.state.activeTabValue}>
            <TabPane tabId={ClipSource.RecordAudio}>
              {this.recordAndPlayTab()}
            </TabPane>
            <TabPane tabId={ClipSource.RecordVideo}>
              {this.state.activeTabValue === ClipSource.RecordVideo ? (
                <VideoEditor
                  onRecorded={this.onVideoRecorded}
                  onReset={this.onResetVideo}
                />
              ) : null}
            </TabPane>
            <TabPane tabId={ClipSource.Upload}>{this.uploadTab()}</TabPane>
          </TabContent>
        </Loader>
      </div>
    );
  }
}
