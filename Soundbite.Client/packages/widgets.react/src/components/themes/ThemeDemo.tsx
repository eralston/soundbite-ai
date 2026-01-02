/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, {
  Component,
  MutableRefObject,
  useEffect,
  useRef,
  useState,
} from "react";
import makeAnimated from "react-select/animated";
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Col,
  FormGroup,
  InputGroup,
  Row,
  Spinner,
} from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faPlay,
  faSave,
  faTimesCircle,
} from "@fortawesome/free-solid-svg-icons";

import {
  Person,
  PersonRole,
  ProviderType,
  Recurrence,
  Series,
  Session,
  SessionCommentPolicy,
  SessionSecurityType,
  SessionType,
  User,
  UserRole,
} from "@soundbite/api";

import {
  FaPrepend,
  LabelInfo,
  PlayButton,
  RichText,
  SbProgress,
  Toggle,
} from "../controls";
import {
  IParticipantOption,
  ParticipantPicker,
  SchedulePicker,
} from "../pickers";
import { lightTheme, GlobalTheme } from "../../styles";
import { RecordAndPlay } from "../RecordAndPlay";
import { MediaPlayerContext } from "../video/context/MediaPlayerContext";
import { Scrubber2 } from "../SoundScrubber2";
import { IMediaPlayerContext } from "../video/context/IMediaPlayerContext";

const baseStyles = css`
  table.sb-theme-table {
    border-collapse: separate !important;
    width: 100%;
  }

  table.sb-theme-table td {
    border-radius: 0.15em;
    padding-left: 1rem;
  }

  .sb-color-background {
    padding: 20px;
    border-radius: 0.35em;
  }

  .color-swatch-header {
    border: 1px solid ${GlobalTheme.current.colors.neutrals.max};
  }

  .sb-swatch-gradient {
    text-transform: capitalize;
  }

  .sb-button-set button,
  .sb-button-set h4 {
    text-transform: capitalize;
  }
`;

interface IDemoPlayerProps {
  disabled?: boolean;
}

const DemoPlayer: React.FC<IDemoPlayerProps> = (props: IDemoPlayerProps) => {
  const [context] = useState<IMediaPlayerContext>(
    () => new MediaPlayerContext("audio")
  );

  useEffect(() => {
    context.load(undefined, "/audio/Martin Luther King.mp3");
  }, []);

  return (
    <div className="sb-card">
      <PlayButton context={context} disabled={props.disabled} />
      <Scrubber2 context={context} disabled={props.disabled} />
    </div>
  );
};

interface IThemeDemoState {
  tick: number;
}

export class ThemeDemo extends Component<{}, IThemeDemoState> {
  constructor(props: {}) {
    super(props);

    this.state = { tick: 0 };
    setInterval(() => {
      this.setState({ tick: this.state.tick + 1 });
    }, 1000);
  }

  neutralRow(name: string, backColor: string, textColor: string) {
    return (
      <tr
        style={{
          backgroundColor: backColor,
          color: textColor,
        }}
      >
        <td>
          {name} {backColor}
        </td>
      </tr>
    );
  }

  swatch(name: string, color: string) {
    return (
      <div className="col-md-3 col-lg-2">
        <div className="color-swatch">
          <div
            className="color-swatch-header"
            style={{
              backgroundColor: color,
            }}
          ></div>
          <div className="color-swatch-body">
            {name} {color}
          </div>
        </div>
      </div>
    );
  }

  gradient(style: string) {
    return (
      <div className={`bg-gradient-${style} p-2 sb-swatch-gradient`}>
        <p>{style}</p>
      </div>
    );
  }

  gradientExamples() {
    return (
      <div>
        <h2>Gradients</h2>
        {this.gradient("primary")}
        {this.gradient("brand")}
        {this.gradient("secondary")}
        {this.gradient("default")}
        {this.gradient("info")}
        {this.gradient("success")}
        {this.gradient("warning")}
        {this.gradient("danger")}
        {this.gradient("white")}
        {this.gradient("light")}
        {this.gradient("neutral")}
        {this.gradient("dark")}
        {this.gradient("darker")}
      </div>
    );
  }

  colors() {
    return (
      <div className="sb-card">
        <h2>Colors</h2>
        <div
          style={{
            backgroundColor: GlobalTheme.current.colors.neutrals.min,
          }}
          className="sb-color-background"
        >
          <h3>Brand</h3>
          <Row>
            {this.swatch("First", GlobalTheme.current.colors.brand.first)}
            {this.swatch("Second", GlobalTheme.current.colors.brand.second)}
          </Row>
          <h3>Bootstrap</h3>
          <Row>
            {this.swatch(
              "Default",
              GlobalTheme.current.colors.bootstrap.default
            )}
            {this.swatch(
              "Primary",
              GlobalTheme.current.colors.bootstrap.primary
            )}
            {this.swatch(
              "Secondary",
              GlobalTheme.current.colors.bootstrap.secondary
            )}
            {this.swatch("Info", GlobalTheme.current.colors.bootstrap.info)}
            {this.swatch(
              "Success",
              GlobalTheme.current.colors.bootstrap.success
            )}
            {this.swatch("Danger", GlobalTheme.current.colors.bootstrap.danger)}
            {this.swatch(
              "Warning",
              GlobalTheme.current.colors.bootstrap.warning
            )}
          </Row>
          <h2>Hues</h2>
          <Row>
            {this.swatch("Red", GlobalTheme.current.colors.hues.red)}
            {this.swatch("Orange", GlobalTheme.current.colors.hues.orange)}
            {this.swatch("Yellow", GlobalTheme.current.colors.hues.yellow)}
            {this.swatch("Green", GlobalTheme.current.colors.hues.green)}
            {this.swatch("Blue", GlobalTheme.current.colors.hues.blue)}
            {this.swatch("Indigo", GlobalTheme.current.colors.hues.indigo)}
            {this.swatch("Violet", GlobalTheme.current.colors.hues.violet)}
            {this.swatch("Pink", GlobalTheme.current.colors.hues.pink)}
            {this.swatch("Teal", GlobalTheme.current.colors.hues.teal)}
            {this.swatch("Cyan", GlobalTheme.current.colors.hues.cyan)}
          </Row>
          <h2>Neutrals</h2>
          <Row>
            <Col md={4}>
              <div className="color-swatch">
                <div
                  className="color-swatch-header"
                  style={{
                    backgroundColor:
                      GlobalTheme.current.colors.neutrals.background,
                  }}
                ></div>
                <div className="color-swatch-body">
                  Background {GlobalTheme.current.colors.neutrals.background}
                </div>
              </div>
            </Col>
            <Col md={4}>
              <div className="color-swatch">
                <div
                  className="color-swatch-header"
                  style={{
                    backgroundColor:
                      GlobalTheme.current.colors.neutrals.midground,
                  }}
                ></div>
                <div className="color-swatch-body">
                  Midground {GlobalTheme.current.colors.neutrals.midground}
                </div>
              </div>
            </Col>
            <Col md={4}>
              <div className="color-swatch">
                <div
                  className="color-swatch-header"
                  style={{
                    backgroundColor:
                      GlobalTheme.current.colors.neutrals.foreground,
                  }}
                ></div>
                <div className="color-swatch-body">
                  Foreground {GlobalTheme.current.colors.neutrals.foreground}
                </div>
              </div>
            </Col>
          </Row>
          <table className="sb-theme-table rounded">
            <tbody>
              {this.neutralRow(
                "Min",
                GlobalTheme.current.colors.neutrals.min,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "100",
                GlobalTheme.current.colors.neutrals.n100,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "200",
                GlobalTheme.current.colors.neutrals.n200,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "300",
                GlobalTheme.current.colors.neutrals.n300,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "400",
                GlobalTheme.current.colors.neutrals.n400,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "500",
                GlobalTheme.current.colors.neutrals.n500,
                GlobalTheme.current.colors.neutrals.max
              )}
              {this.neutralRow(
                "600",
                GlobalTheme.current.colors.neutrals.n600,
                GlobalTheme.current.colors.neutrals.min
              )}
              {this.neutralRow(
                "700",
                GlobalTheme.current.colors.neutrals.n700,
                GlobalTheme.current.colors.neutrals.min
              )}
              {this.neutralRow(
                "800",
                GlobalTheme.current.colors.neutrals.n800,
                GlobalTheme.current.colors.neutrals.min
              )}
              {this.neutralRow(
                "900",
                GlobalTheme.current.colors.neutrals.n900,
                GlobalTheme.current.colors.neutrals.min
              )}
              {this.neutralRow(
                "max",
                GlobalTheme.current.colors.neutrals.max,
                GlobalTheme.current.colors.neutrals.min
              )}
            </tbody>
          </table>
          {this.gradientExamples()}
        </div>
      </div>
    );
  }

  typography(): React.ReactNode {
    return (
      <div className="mb-4 p-2 sb-card">
        <h2>Typography</h2>
        <Row>
          <Col md="4">
            <h4>Font Family</h4>
            <p>{GlobalTheme.current.fonts.fontFamily}</p>
            <h4>Mono Font Family</h4>
            <p style={{ fontFamily: GlobalTheme.current.fonts.fontFamilyMono }}>
              {GlobalTheme.current.fonts.fontFamilyMono}
            </p>
          </Col>
          <Col md="4">
            <h4>Headers</h4>
            <h1>Header 1</h1>
            <h2>Header 2</h2>
            <h3>Header 3</h3>
            <h4>Header 4</h4>
            <h5>Header 5</h5>
          </Col>
          <Col md="4">
            <h4>Normal Text</h4>
            <p>The quick brown fox jumps over the lazy dog</p>
            <h4>Muted Text</h4>
            <p className="text-muted">
              The quick brown fox jumps over the lazy dog
            </p>
          </Col>
        </Row>
      </div>
    );
  }

  buttonSet(color: string) {
    return (
      <div className="sb-button-set mt-3">
        <h4>{color}</h4>
        <Row>
          <Col lg={4} className="mt-1">
            <Button color={color}>Normal</Button>
            <Button color={color} className="hover">
              Hover
            </Button>
          </Col>
          <Col lg={4} className="mt-1">
            <Button color={color} className="focus active">
              Focus
            </Button>
            <Button color={color} disabled={true}>
              Disabled
            </Button>
          </Col>
          <Col lg={4} className="mt-1">
            <Button color={color} outline={true}>
              Outline
            </Button>
            <Button color={color} outline={true} disabled={true}>
              Disabled
            </Button>
          </Col>
        </Row>
      </div>
    );
  }

  buttons() {
    return (
      <div className="mb-4 sb-card">
        <h2>Buttons</h2>
        <Row className="mb-4">
          <Col>
            {this.buttonSet("default")}
            {this.buttonSet("primary")}
            {this.buttonSet("secondary")}
            {this.buttonSet("info")}
            {this.buttonSet("success")}
            {this.buttonSet("warning")}
            {this.buttonSet("danger")}
          </Col>
        </Row>
        <h4>Block Buttons</h4>
        <Row className="mb-4">
          <Col md="4">
            <Button className="btn-icon btn-block" color="primary">
              Save &amp; Close
            </Button>
          </Col>
          <Col md="4">
            <Button className="btn-icon btn-block" color="danger">
              <span className="btn-inner--icon  mr-1">
                <FontAwesomeIcon icon={faTimesCircle} />
              </span>
              <span className="btn-inner--text">Icon Button</span>
            </Button>
          </Col>
        </Row>
        <Row className="mb-4">
          <Col md="4">
            <h4>Toggle</h4>
            <Toggle />
          </Col>
          <Col md="4">
            <h4>Toggle Disabled</h4>
            <Toggle disabled={true} />
          </Col>
          <Col md="4">
            <h4>Spinner</h4>
            <Spinner type="grow" className="sb-spinner-default" />
          </Col>
        </Row>
      </div>
    );
  }

  cards() {
    return (
      <div className="mb-4">
        <h2>Cards</h2>
        <Row className="mb-3">
          <Col md="6">
            <Card>
              <CardHeader className="bg-transparent">
                <Row className="align-items-center">
                  <Col>
                    <h1 className="mb-0">Example Card</h1>
                  </Col>
                </Row>
              </CardHeader>
              <CardBody>
                <FormGroup>
                  <InputGroup>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="Example Form Control"
                      aria-label="Example Form Control"
                    />
                  </InputGroup>
                </FormGroup>
                <FormGroup>
                  <textarea
                    className="form-control"
                    placeholder="Example Text Area"
                  ></textarea>
                </FormGroup>
                <Row>
                  <Col>
                    <Button
                      className="btn-icon btn-3 btn-block"
                      type="button"
                      color="secondary"
                    >
                      <span className="btn-inner--icon mr-1">
                        <FontAwesomeIcon icon={faTimesCircle} />
                      </span>
                      <span className="btn-inner--text">Cancel</span>
                    </Button>
                  </Col>
                  <Col>
                    <Button
                      className="btn-icon btn-3 btn-block"
                      type="button"
                      color="primary"
                    >
                      <span className="btn-inner--icon  mr-1">
                        <FontAwesomeIcon icon={faSave} />
                      </span>
                      <span className="btn-inner--text">Save</span>
                    </Button>
                  </Col>
                </Row>
              </CardBody>
            </Card>
          </Col>
          <Col md="6">
            <Card>
              <CardHeader className="bg-transparent">
                <Row className="align-items-center">
                  <Col>
                    <h1 className="mb-0">Example Table Card</h1>
                  </Col>
                </Row>
              </CardHeader>
              <div className="table-responsive">
                <table className="sb-max-width-table table align-items-center">
                  <thead className="thead-light">
                    <tr>
                      <th scope="col" className="sort" data-sort="name">
                        Name
                      </th>
                      <th
                        scope="col"
                        className="sort d-none d-md-table-cell"
                        data-sort="publish"
                      >
                        Publish
                      </th>
                      <th
                        scope="col"
                        className="sort"
                        data-sort="action"
                        style={{ width: "166px" }}
                      ></th>
                    </tr>
                  </thead>
                  <tbody className="list">
                    <tr>
                      <th>
                        CEO Update
                        <div className="d-table-cell d-md-none">
                          <span className="text-muted sb-cell-detail">
                            05/27/2021
                          </span>
                        </div>
                      </th>
                      <td className="d-none d-md-table-cell">
                        <span className="">05/27/2021</span>
                      </td>
                      <td className="text-right">
                        <button
                          className="btn btn-outline-primary btn-sm sb-play-btn"
                          type="button"
                        >
                          <span className="btn-inner--icon">
                            <FontAwesomeIcon icon={faPlay} />
                            <span className="btn-inner--text">Play</span>
                          </span>
                        </button>
                      </td>
                    </tr>
                    <tr>
                      <th>
                        Welcome!
                        <div className="d-table-cell d-md-none">
                          <span className="text-muted sb-cell-detail">
                            05/26/2021
                          </span>
                        </div>
                      </th>
                      <td className="d-none d-md-table-cell">
                        <span className="">05/26/2021</span>
                      </td>
                      <td className="text-right">
                        <button
                          className="btn btn-outline-primary btn-sm sb-play-btn"
                          type="button"
                        >
                          <span className="btn-inner--icon">
                            <FontAwesomeIcon icon={faPlay} />
                            <span className="btn-inner--text">Play</span>
                          </span>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </Card>
          </Col>
        </Row>
      </div>
    );
  }

  animatedComponents = makeAnimated();

  fakeUser(givenName: string, familyName: string) {
    const user: User = {
      email: "Hello@World.Com",
      givenName: givenName,
      familyName: familyName,
      phone: "509-312-9058",
      title: "",
      allowNews: true,
      allowMarketing: true,
      allowEmail: true,
      allowSms: true,
      userRole: UserRole.User,
      providerType: ProviderType.AAD,
      calendarSettings: "",
      isAccepting: true,
      imageSrc: "",
      route: "",
      createdUtc: "",
      updatedUtc: "",
      upn: "",
    };
    return user;
  }

  fakePerson(givenName: string, familyName: string) {
    const user = this.fakeUser(givenName, familyName);
    const person = {
      personRole: PersonRole.Person,
      user: user,
      route: "",
      createdUtc: "",
      updatedUtc: "",
    };
    return person;
  }

  participantPickers() {
    const allPeople: Person[] = [
      this.fakePerson("Fooey", "Barla"),
      this.fakePerson("Alice", "Aardvark"),
      this.fakePerson("Bob", "Beaver"),
      this.fakePerson("Clarissa", " Zebra"),
    ];
    const selectedPeople = [allPeople[0], allPeople[1]];
    const selectedPeopleOptions: IParticipantOption[] =
      ParticipantPicker.peopleToOptions(selectedPeople);
    selectedPeopleOptions[0].isFixed = true;

    return (
      <div className="mb-4">
        <Row>
          <Col md="6">
            <h4>Participant Picker</h4>
            <ParticipantPicker
              defaultParticipants={selectedPeopleOptions}
              people={allPeople}
            />
          </Col>
          <Col md="6">
            <h4>Disabled Participant Picker</h4>
            <ParticipantPicker
              defaultParticipants={selectedPeopleOptions}
              people={allPeople}
              disabled={true}
            />
          </Col>
        </Row>
      </div>
    );
  }

  inputs() {
    const session: Session & Series = {
      name: "Example",
      limit: 120,
      sessionType: SessionType.Announcement,
      sessionSecurity: SessionSecurityType.Protected,
      route: "",
      createdUtc: "",
      updatedUtc: "",
      reminderCalEventId: "",
      recurrence: Recurrence.NoRepeat,
      recurrenceData: "",
      transcribe: true,
      sessionCommentPolicy: SessionCommentPolicy.Allowed,
    };
    return (
      <div>
        <Row>
          <Col md={6}>
            <FormGroup>
              <h4>Text Input</h4>
              <InputGroup aria-disabled={false}>
                <FaPrepend icon="pen" />
                <input
                  type="text"
                  className="form-control"
                  placeholder="Title"
                  aria-label="Title"
                  aria-describedby="basic-addon1"
                  disabled={false}
                />
              </InputGroup>
            </FormGroup>
          </Col>
          <Col md={6}>
            <FormGroup>
              <h4>Disabled Text Input</h4>
              <InputGroup aria-disabled={true}>
                <FaPrepend icon="pen" />
                <input
                  type="text"
                  className="form-control"
                  placeholder="Title"
                  aria-label="Title"
                  aria-describedby="basic-addon1"
                  disabled={true}
                />
              </InputGroup>
            </FormGroup>
          </Col>
        </Row>
        <Row>
          <Col md={6}>
            <h4>Rich Text</h4>
            <FormGroup>
              <LabelInfo
                label="Call to Action"
                info="Description shared with all participants"
              />
              <RichText placeholder="Call to Action for this Soundbite Session" />
            </FormGroup>
          </Col>
          <Col md={6}>
            <h4>Disabled Rich Text</h4>
            <FormGroup>
              <LabelInfo
                label="Call to Action"
                info="Description shared with all participants"
              />
              <RichText
                placeholder="Call to Action for this Soundbite Session"
                disabled={true}
              />
            </FormGroup>
          </Col>
        </Row>
        <Row>
          <Col md={6}>
            <h4>Schedule Picker</h4>
            <SchedulePicker session={session} />
          </Col>
          <Col md={6}>
            <h4>Disabled Schedule Picker</h4>
            <SchedulePicker session={session} disabled={true} />
          </Col>
        </Row>
        <Row>
          <Col md={6}>
            <h4>Progress Bar - No Title, No Progress</h4>
            <SbProgress progress={0} />
            <h4>Progress Bar - No Title, Progress</h4>
            <SbProgress progress={60} />
            <h4>Progress Bar - No Title, Completed</h4>
            <SbProgress progress={100} />
            <h4>Progress Bar - No Title, Progressing</h4>
            <SbProgress progress={this.state.tick % 100} />
          </Col>
          <Col md={6}>
            <h4>Progress Bar - Title, No Progress</h4>
            <SbProgress title="Example Title..." progress={0} />
            <h4>Progress Bar - Title, Progress</h4>
            <SbProgress title="Example Title..." progress={60} />
            <h4>Progress Bar - Title, Completed</h4>
            <SbProgress title="Example Title..." progress={100} />
            <h4>Progress Bar - Title, Progressing</h4>
            <SbProgress
              title="Example Title..."
              progress={this.state.tick % 100}
            />
          </Col>
        </Row>
      </div>
    );
  }

  forms() {
    return (
      <div className="mb-4 sb-card">
        <h2>Forms</h2>
        <div className="bg-midground p-4">
          {this.participantPickers()}
          {this.inputs()}
        </div>
      </div>
    );
  }

  isLightActive() {
    return GlobalTheme.current.name === lightTheme.name;
  }

  sound() {
    return (
      <div>
        <h4>Player</h4>
        <Row>
          <Col md={6}>
            <DemoPlayer />
          </Col>
          <Col md={6}>
            <DemoPlayer disabled={true} />
          </Col>
        </Row>
        <h4>Recorder</h4>
        <Row>
          <Col md={6}>
            <div className="sb-card">
              <RecordAndPlay />
            </div>
          </Col>
          <Col md={6}>
            <div className="sb-card">
              <RecordAndPlay disabled={true} />
            </div>
          </Col>
        </Row>
      </div>
    );
  }

  alerts() {
    return (
      <div className="sb-card">
        <h2>Alerts</h2>
        <div
          className="alert-default alert alert-success fade show"
          role="alert"
        >
          <strong>Default!</strong> This is a default alert - check it out!
        </div>
        <div className="alert alert-primary fade show" role="alert">
          <strong>Primary!</strong> This is a primary alert - check it out!
        </div>
        <div className="alert alert-secondary fade show" role="alert">
          <strong>Secondary!</strong> This is a secondary alert - check it out!
        </div>
        <div className="alert alert-info fade show" role="alert">
          <strong>Info!</strong> This is a info alert - check it out!
        </div>
        <div className="alert alert-success fade show" role="alert">
          <strong>Success!</strong> This is a success alert - check it out!
        </div>
        <div className="alert alert-danger fade show" role="alert">
          <strong>Danger!</strong> This is a danger alert - check it out!
        </div>
        <div className="alert alert-warning fade show" role="alert">
          <strong>Warning!</strong> This is a warning alert - check it out!
        </div>
      </div>
    );
  }

  logos() {
    return (
      <div className="sb-card">
        <h2>Logos</h2>
        <Row>
          <Col md="6" className="pb-4">
            <h3>Logo</h3>
            <p className="text-muted">
              Main Logo. EG, top of the left nav bar in Soundbite Studio
            </p>
            <img
              alt="Soundbite"
              className=""
              src={GlobalTheme.current.images.logo}
            />
          </Col>
          <Col md="6" className="pb-4">
            <h3>Mark</h3>
            <p className="text-muted">
              Brand icon w/o words. EG, top of the mobile nav bar in Soundbite
              Studio
            </p>
            <img
              alt="Soundbite"
              className=""
              src={GlobalTheme.current.images.mark}
            />
          </Col>
        </Row>
        <Row>
          <Col>
            <h3>Alt Logo</h3>
            <p className="text-muted">
              Color reversed logo. EG, the top of the login view in Soundbite
              Studio
            </p>
          </Col>
        </Row>
        <Row>
          <Col>
            <div className="rounded sb-progress-background mb-2">
              <img
                alt="Soundbite"
                className=""
                src={GlobalTheme.current.images.logoAlt}
              />
            </div>
          </Col>
        </Row>
        <Row>
          <Col>
            <div className="rounded bg-dark">
              <img
                alt="Soundbite"
                className=""
                src={GlobalTheme.current.images.logoAlt}
              />
            </div>
          </Col>
        </Row>
        <Row className="pb-4">
          <Col></Col>
        </Row>
      </div>
    );
  }

  render() {
    return (
      <div css={baseStyles}>
        {this.sound()}
        {this.forms()}
        {this.cards()}
        {this.buttons()}
        {this.typography()}
        {this.alerts()}
        {this.colors()}
        {this.logos()}
      </div>
    );
  }
}
